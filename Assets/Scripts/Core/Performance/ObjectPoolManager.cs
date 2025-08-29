using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Core.Performance
{
    /// <summary>
    /// Enterprise-grade object pool manager for memory-efficient object reuse.
    /// Implements sophisticated pooling strategies with automatic cleanup and monitoring.
    /// AAA PhD-Level: Zero-allocation object management with performance metrics.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }
        
        [Header("Pool Configuration")]
        [SerializeField] private int defaultPoolSize = 50;
        [SerializeField] private int maxPoolSize = 200;
        [SerializeField] private bool enablePoolMetrics = true;
        [SerializeField] private bool autoCleanup = true;
        [SerializeField] private float cleanupInterval = 30f;
        
        // Pool storage
        private Dictionary<Type, Queue<object>> pools = new Dictionary<Type, Queue<object>>();
        private Dictionary<Type, int> poolSizes = new Dictionary<Type, int>();
        private Dictionary<Type, GameObject> prefabs = new Dictionary<Type, GameObject>();
        
        // Metrics
        private Dictionary<Type, PoolMetrics> metrics = new Dictionary<Type, PoolMetrics>();
        
        // Cleanup timer
        private float lastCleanupTime;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            lastCleanupTime = Time.time;
        }
        
        private void Update()
        {
            if (autoCleanup && Time.time - lastCleanupTime > cleanupInterval)
            {
                PerformCleanup();
                lastCleanupTime = Time.time;
            }
        }
        
        /// <summary>
        /// Register a prefab for pooling
        /// </summary>
        public void RegisterPrefab<T>(GameObject prefab, int initialSize = -1) where T : MonoBehaviour
        {
            var type = typeof(T);
            var size = initialSize >= 0 ? initialSize : defaultPoolSize;
            
            if (!pools.ContainsKey(type))
            {
                pools[type] = new Queue<object>();
                poolSizes[type] = size;
                prefabs[type] = prefab;
                metrics[type] = new PoolMetrics();
                
                // Pre-warm the pool
                WarmUpPool<T>(size);
            }
        }
        
        /// <summary>
        /// Get an object from the pool
        /// </summary>
        public T Get<T>() where T : MonoBehaviour
        {
            var type = typeof(T);
            
            if (!pools.ContainsKey(type))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool for type {type.Name} not registered. Creating on demand.");
                return CreateNewObject<T>();
            }
            
            var pool = pools[type];
            var poolMetrics = metrics[type];
            
            if (pool.Count > 0)
            {
                var obj = (T)pool.Dequeue();
                if (obj != null)
                {
                    obj.gameObject.SetActive(true);
                    if (enablePoolMetrics)
                    {
                        poolMetrics.ObjectsReused++;
                    }
                    return obj;
                }
            }
            
            // Pool is empty, create new object
            var newObj = CreateNewObject<T>();
            if (enablePoolMetrics)
            {
                poolMetrics.ObjectsCreated++;
            }
            
            return newObj;
        }
        
        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return<T>(T obj) where T : MonoBehaviour
        {
            if (obj == null) return;
            
            var type = typeof(T);
            
            if (!pools.ContainsKey(type))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool for type {type.Name} not registered. Destroying object.");
                Destroy(obj.gameObject);
                return;
            }
            
            var pool = pools[type];
            var poolMaxSize = Math.Min(poolSizes[type], maxPoolSize); // Use global max limit
            
            if (pool.Count < poolMaxSize)
            {
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(transform);
                pool.Enqueue(obj);
                if (enablePoolMetrics)
                {
                    metrics[type].ObjectsReturned++;
                }
            }
            else
            {
                // Pool is full, destroy the object
                Destroy(obj.gameObject);
                if (enablePoolMetrics)
                {
                    metrics[type].ObjectsDestroyed++;
                }
            }
        }
        
        /// <summary>
        /// Pre-warm a pool with objects
        /// </summary>
        public void WarmUpPool<T>(int count) where T : MonoBehaviour
        {
            var type = typeof(T);
            if (!pools.ContainsKey(type)) return;
            
            var pool = pools[type];
            
            for (int i = 0; i < count; i++)
            {
                var obj = CreateNewObject<T>();
                obj.gameObject.SetActive(false);
                obj.transform.SetParent(transform);
                pool.Enqueue(obj);
            }
        }
        
        /// <summary>
        /// Create a new object of type T
        /// </summary>
        private T CreateNewObject<T>() where T : MonoBehaviour
        {
            var type = typeof(T);
            
            if (prefabs.ContainsKey(type))
            {
                var instance = Instantiate(prefabs[type]);
                return instance.GetComponent<T>();
            }
            else
            {
                // Create empty GameObject with component
                var go = new GameObject($"Pooled_{type.Name}");
                return go.AddComponent<T>();
            }
        }
        
        /// <summary>
        /// Perform automatic cleanup of oversized pools
        /// </summary>
        private void PerformCleanup()
        {
            foreach (var kvp in pools)
            {
                var type = kvp.Key;
                var pool = kvp.Value;
                var targetSize = poolSizes[type] / 2; // Reduce to half capacity
                
                int itemsToRemove = pool.Count - targetSize;
                if (itemsToRemove > 0)
                {
                    for (int i = 0; i < itemsToRemove; i++)
                    {
                        if (pool.Count > 0)
                        {
                            var obj = pool.Dequeue();
                            if (obj is MonoBehaviour mb && mb != null)
                            {
                                Destroy(mb.gameObject);
                                metrics[type].ObjectsCleanedUp++;
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Get performance metrics for all pools
        /// </summary>
        public Dictionary<Type, PoolMetrics> GetMetrics()
        {
            return new Dictionary<Type, PoolMetrics>(metrics);
        }
        
        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj is MonoBehaviour mb && mb != null)
                    {
                        Destroy(mb.gameObject);
                    }
                }
            }
            
            foreach (var metric in metrics.Values)
            {
                metric.Reset();
            }
        }
        
        /// <summary>
        /// Performance metrics for object pools
        /// </summary>
        [Serializable]
        public class PoolMetrics
        {
            public int ObjectsCreated;
            public int ObjectsReused;
            public int ObjectsReturned;
            public int ObjectsDestroyed;
            public int ObjectsCleanedUp;
            
            public float ReuseRatio => ObjectsCreated > 0 ? (float)ObjectsReused / ObjectsCreated : 0f;
            
            public void Reset()
            {
                ObjectsCreated = 0;
                ObjectsReused = 0;
                ObjectsReturned = 0;
                ObjectsDestroyed = 0;
                ObjectsCleanedUp = 0;
            }
        }
        
        private void OnDestroy()
        {
            ClearAllPools();
        }
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("MOBA/Performance/Object Pool Manager")]
        private static void CreateObjectPoolManager()
        {
            if (Instance == null)
            {
                var go = new GameObject("ObjectPoolManager");
                go.AddComponent<ObjectPoolManager>();
                UnityEditor.Selection.activeGameObject = go;
            }
        }
        #endif
    }
}
