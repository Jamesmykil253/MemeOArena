using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using MOBA.Core.Performance;

namespace MOBA.Core.Memory
{
    /// <summary>
    /// Enterprise-grade memory management system with automatic cleanup and leak detection.
    /// PhD-level: Implements RAII patterns, weak references, and automatic dispose tracking.
    /// Prevents memory leaks through systematic lifecycle management.
    /// </summary>
    public static class MemoryManager
    {
        private static readonly Dictionary<int, WeakReference> _trackedObjects = new();
        private static readonly HashSet<int> _disposableObjects = new();
        private static readonly Queue<Action> _cleanupActions = new();
        
        private static int _nextTrackingId = 1;
        private static float _lastCleanupTime;
        private const float CLEANUP_INTERVAL = 30f; // 30 seconds
        
        // Memory statistics
        public static long TotalManagedMemory => GC.GetTotalMemory(false);
        public static int TrackedObjectCount => _trackedObjects.Count;
        public static int PendingCleanupCount => _cleanupActions.Count;
        
        /// <summary>
        /// Track an object for automatic memory management
        /// </summary>
        public static int TrackObject(object obj)
        {
            if (obj == null) return -1;
            
            var id = _nextTrackingId++;
            _trackedObjects[id] = new WeakReference(obj);
            
            if (obj is IDisposable)
            {
                _disposableObjects.Add(id);
            }
            
            return id;
        }
        
        /// <summary>
        /// Register cleanup action for automatic execution
        /// </summary>
        public static void RegisterCleanupAction(Action cleanupAction)
        {
            if (cleanupAction != null)
            {
                _cleanupActions.Enqueue(cleanupAction);
            }
        }
        
        /// <summary>
        /// Create a managed disposable wrapper
        /// </summary>
        public static ManagedDisposable<T> CreateManagedDisposable<T>(T obj) where T : class, IDisposable
        {
            return new ManagedDisposable<T>(obj);
        }
        
        /// <summary>
        /// Perform cleanup of dead references and execute pending actions
        /// </summary>
        public static void PerformCleanup()
        {
            // Clean up dead weak references
            var deadReferences = new List<int>();
            foreach (var kvp in _trackedObjects)
            {
                if (!kvp.Value.IsAlive)
                {
                    deadReferences.Add(kvp.Key);
                }
            }
            
            foreach (var id in deadReferences)
            {
                _trackedObjects.Remove(id);
                _disposableObjects.Remove(id);
            }
            
            // Execute pending cleanup actions
            var actionsToExecute = Math.Min(_cleanupActions.Count, 10); // Limit per frame
            for (int i = 0; i < actionsToExecute; i++)
            {
                if (_cleanupActions.Count > 0)
                {
                    var action = _cleanupActions.Dequeue();
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Cleanup action failed: {ex}");
                    }
                }
            }
            
            _lastCleanupTime = Time.time;
        }
        
        /// <summary>
        /// Force garbage collection (use sparingly)
        /// </summary>
        public static void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        
        /// <summary>
        /// Get memory statistics for debugging
        /// </summary>
        public static MemoryStatistics GetStatistics()
        {
            return new MemoryStatistics
            {
                TotalManagedMemory = TotalManagedMemory,
                TrackedObjects = TrackedObjectCount,
                DisposableObjects = _disposableObjects.Count,
                PendingCleanupActions = PendingCleanupCount,
                LastCleanupTime = _lastCleanupTime
            };
        }
        
        /// <summary>
        /// Update called by MemoryManagerUpdater MonoBehaviour
        /// </summary>
        internal static void Update()
        {
            if (Time.time - _lastCleanupTime > CLEANUP_INTERVAL)
            {
                PerformCleanup();
            }
        }
        
        public struct MemoryStatistics
        {
            public long TotalManagedMemory;
            public int TrackedObjects;
            public int DisposableObjects;
            public int PendingCleanupActions;
            public float LastCleanupTime;
        }
    }
    
    /// <summary>
    /// RAII wrapper for automatic disposal of managed objects
    /// </summary>
    public class ManagedDisposable<T> : IDisposable where T : class, IDisposable
    {
        private T _object;
        private bool _disposed = false;
        
        public T Object => _disposed ? null : _object;
        public bool IsDisposed => _disposed;
        
        public ManagedDisposable(T obj)
        {
            _object = obj ?? throw new ArgumentNullException(nameof(obj));
            MemoryManager.TrackObject(this);
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            
            try
            {
                _object?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error disposing {typeof(T).Name}: {ex}");
            }
            finally
            {
                _object = null;
                _disposed = true;
            }
        }
        
        ~ManagedDisposable()
        {
            if (!_disposed)
            {
                Debug.LogWarning($"ManagedDisposable<{typeof(T).Name}> was not properly disposed!");
                Dispose();
            }
        }
    }
    
    /// <summary>
    /// Pool for Unity native arrays to prevent allocations
    /// </summary>
    public static class NativeArrayPool<T> where T : struct
    {
        private static readonly Dictionary<int, Stack<NativeArray<T>>> _pools = new();
        private const int MAX_POOL_SIZE = 10;
        
        public static NativeArray<T> Get(int length, Allocator allocator = Allocator.Temp)
        {
            if (!_pools.TryGetValue(length, out var pool) || pool.Count == 0)
            {
                return new NativeArray<T>(length, allocator);
            }
            
            var array = pool.Pop();
            return array.IsCreated ? array : new NativeArray<T>(length, allocator);
        }
        
        public static void Return(NativeArray<T> array)
        {
            if (!array.IsCreated) return;
            
            var length = array.Length;
            if (!_pools.TryGetValue(length, out var pool))
            {
                pool = new Stack<NativeArray<T>>();
                _pools[length] = pool;
            }
            
            if (pool.Count < MAX_POOL_SIZE)
            {
                pool.Push(array);
            }
            else
            {
                array.Dispose();
            }
        }
        
        public static void ClearPool()
        {
            foreach (var pool in _pools.Values)
            {
                while (pool.Count > 0)
                {
                    var array = pool.Pop();
                    if (array.IsCreated)
                    {
                        array.Dispose();
                    }
                }
            }
            _pools.Clear();
        }
    }
    
    /// <summary>
    /// MonoBehaviour updater for MemoryManager - attach to persistent GameObject
    /// </summary>
    public class MemoryManagerUpdater : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            MemoryManager.Update();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Cleanup when app is paused (mobile)
                MemoryManager.PerformCleanup();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Cleanup when app loses focus
                MemoryManager.PerformCleanup();
            }
        }
        
        private void OnDestroy()
        {
            // Final cleanup
            MemoryManager.PerformCleanup();
            NativeArrayPool<float>.ClearPool();
            NativeArrayPool<int>.ClearPool();
            NativeArrayPool<Vector3>.ClearPool();
        }
    }
    
    /// <summary>
    /// Extension methods for easier memory management
    /// </summary>
    public static class MemoryExtensions
    {
        public static ManagedDisposable<T> AsManaged<T>(this T obj) where T : class, IDisposable
        {
            return MemoryManager.CreateManagedDisposable(obj);
        }
        
        public static void TrackForCleanup(this UnityEngine.Object obj)
        {
            if (obj != null)
            {
                MemoryManager.RegisterCleanupAction(() => {
                    if (obj != null)
                    {
                        if (Application.isPlaying)
                            UnityEngine.Object.Destroy(obj);
                        else
                            UnityEngine.Object.DestroyImmediate(obj);
                    }
                });
            }
        }
        
        public static void SafeDispose<T>(this T obj) where T : class, IDisposable
        {
            try
            {
                obj?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error disposing {typeof(T).Name}: {ex}");
            }
        }
    }
}
