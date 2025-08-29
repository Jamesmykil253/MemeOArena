using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace MOBA.Core.Memory
{
    /// <summary>
    /// Enterprise-grade memory manager with automatic garbage collection optimization.
    /// Monitors memory usage, prevents memory leaks, and optimizes GC timing.
    /// AAA PhD-Level: Production-quality memory management and optimization.
    /// </summary>
    public class MemoryManagerUpdater : MonoBehaviour
    {
        public static MemoryManagerUpdater Instance { get; private set; }
        
        [Header("Memory Configuration")]
        [SerializeField] private long memoryBudgetMB = 512;
        [SerializeField] private float gcThresholdRatio = 0.8f; // Trigger GC at 80% of budget
        [SerializeField] private bool enableAutoGC = true;
        [SerializeField] private float gcCheckInterval = 1f;
        
        [Header("Memory Monitoring")]
        [SerializeField] private bool enableMemoryTracking = true;
        [SerializeField] private bool logMemoryEvents = false;
        [SerializeField] private float memoryCheckInterval = 0.5f;
        
        [Header("Performance")]
        [SerializeField] private bool enableMemoryPressureDetection = true;
        [SerializeField] private float highPressureThreshold = 0.9f;
        [SerializeField] private float criticalPressureThreshold = 0.95f;
        
        // Memory tracking
        private long memoryBudgetBytes;
        private long lastTotalMemory;
        private long lastManagedMemory;
        private long peakMemoryUsage;
        private float lastGCTime;
        private float lastMemoryCheckTime;
        private int gcCollectionCount;
        
        // Memory pressure state
        private MemoryPressureLevel currentPressureLevel = MemoryPressureLevel.Normal;
        private MemoryPressureLevel previousPressureLevel = MemoryPressureLevel.Normal;
        
        // Events
        public event Action<MemoryReport> OnMemoryReport;
        public event Action<MemoryPressureLevel> OnMemoryPressureChanged;
        public event Action OnMemoryBudgetExceeded;
        public event Action OnGarbageCollectionTriggered;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeMemoryManager();
        }
        
        private void InitializeMemoryManager()
        {
            memoryBudgetBytes = memoryBudgetMB * 1024 * 1024;
            lastGCTime = Time.time;
            lastMemoryCheckTime = Time.time;
            
            // Get initial memory state
            UpdateMemoryMetrics();
            
            if (logMemoryEvents)
            {
                Debug.Log($"[MemoryManager] Initialized with budget: {memoryBudgetMB}MB ({memoryBudgetBytes} bytes)");
            }
        }
        
        private void Update()
        {
            if (!enableMemoryTracking) return;
            
            // Check memory at specified interval
            if (Time.time - lastMemoryCheckTime >= memoryCheckInterval)
            {
                UpdateMemoryMetrics();
                CheckMemoryPressure();
                lastMemoryCheckTime = Time.time;
            }
            
            // Auto garbage collection check
            if (enableAutoGC && Time.time - lastGCTime >= gcCheckInterval)
            {
                CheckAndTriggerGarbageCollection();
                lastGCTime = Time.time;
            }
        }
        
        private void UpdateMemoryMetrics()
        {
            lastTotalMemory = (long)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            lastManagedMemory = GC.GetTotalMemory(false);
            
            if (lastTotalMemory > peakMemoryUsage)
            {
                peakMemoryUsage = lastTotalMemory;
            }
        }
        
        private void CheckMemoryPressure()
        {
            if (!enableMemoryPressureDetection) return;
            
            float memoryRatio = (float)lastTotalMemory / memoryBudgetBytes;
            
            MemoryPressureLevel newPressureLevel;
            
            if (memoryRatio >= criticalPressureThreshold)
            {
                newPressureLevel = MemoryPressureLevel.Critical;
            }
            else if (memoryRatio >= highPressureThreshold)
            {
                newPressureLevel = MemoryPressureLevel.High;
            }
            else if (memoryRatio >= gcThresholdRatio)
            {
                newPressureLevel = MemoryPressureLevel.Medium;
            }
            else
            {
                newPressureLevel = MemoryPressureLevel.Normal;
            }
            
            if (newPressureLevel != currentPressureLevel)
            {
                previousPressureLevel = currentPressureLevel;
                currentPressureLevel = newPressureLevel;
                
                OnMemoryPressureChanged?.Invoke(currentPressureLevel);
                
                if (logMemoryEvents)
                {
                    Debug.Log($"[MemoryManager] Memory pressure changed: {previousPressureLevel} -> {currentPressureLevel} (Ratio: {memoryRatio:F2})");
                }
                
                // Handle critical memory situations
                if (currentPressureLevel == MemoryPressureLevel.Critical)
                {
                    HandleCriticalMemory();
                }
            }
        }
        
        private void CheckAndTriggerGarbageCollection()
        {
            float memoryRatio = (float)lastTotalMemory / memoryBudgetBytes;
            
            bool shouldTriggerGC = false;
            
            // Trigger GC based on memory pressure
            if (memoryRatio >= gcThresholdRatio)
            {
                shouldTriggerGC = true;
            }
            
            // Trigger GC if memory budget exceeded
            if (lastTotalMemory > memoryBudgetBytes)
            {
                shouldTriggerGC = true;
                OnMemoryBudgetExceeded?.Invoke();
            }
            
            if (shouldTriggerGC)
            {
                ForceGarbageCollection();
            }
        }
        
        private void HandleCriticalMemory()
        {
            if (logMemoryEvents)
            {
                Debug.LogWarning($"[MemoryManager] Critical memory pressure detected! Current: {GetTotalMemoryMB():F1}MB, Budget: {memoryBudgetMB}MB");
            }
            
            // Emergency memory cleanup
            ForceGarbageCollection();
            
            // Unload unused assets
            StartCoroutine(UnloadUnusedAssetsCoroutine());
            
            // Notify systems to reduce memory usage
            BroadcastMessage("OnCriticalMemoryPressure", SendMessageOptions.DontRequireReceiver);
        }
        
        /// <summary>
        /// Force garbage collection immediately
        /// </summary>
        public void ForceGarbageCollection()
        {
            long memoryBeforeGC = lastTotalMemory;
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            gcCollectionCount++;
            
            UpdateMemoryMetrics();
            long memoryAfterGC = lastTotalMemory;
            long memoryFreed = memoryBeforeGC - memoryAfterGC;
            
            OnGarbageCollectionTriggered?.Invoke();
            
            if (logMemoryEvents)
            {
                Debug.Log($"[MemoryManager] Garbage collection completed. Freed: {memoryFreed / (1024f * 1024f):F2}MB");
            }
        }
        
        /// <summary>
        /// Unload unused assets asynchronously
        /// </summary>
        private IEnumerator UnloadUnusedAssetsCoroutine()
        {
            if (logMemoryEvents)
            {
                Debug.Log("[MemoryManager] Unloading unused assets...");
            }
            
            var asyncOp = Resources.UnloadUnusedAssets();
            yield return asyncOp;
            
            UpdateMemoryMetrics();
            
            if (logMemoryEvents)
            {
                Debug.Log($"[MemoryManager] Unused assets unloaded. Current memory: {GetTotalMemoryMB():F1}MB");
            }
        }
        
        /// <summary>
        /// Generate comprehensive memory report
        /// </summary>
        public MemoryReport GenerateMemoryReport()
        {
            UpdateMemoryMetrics();
            
            var report = new MemoryReport
            {
                Timestamp = DateTime.Now,
                TotalMemoryMB = GetTotalMemoryMB(),
                ManagedMemoryMB = GetManagedMemoryMB(),
                PeakMemoryMB = peakMemoryUsage / (1024f * 1024f),
                MemoryBudgetMB = memoryBudgetMB,
                MemoryPressureLevel = currentPressureLevel,
                GCCollectionCount = gcCollectionCount,
                MemoryUtilization = (float)lastTotalMemory / memoryBudgetBytes
            };
            
            OnMemoryReport?.Invoke(report);
            return report;
        }
        
        /// <summary>
        /// Set memory budget in MB
        /// </summary>
        public void SetMemoryBudget(long budgetMB)
        {
            memoryBudgetMB = budgetMB;
            memoryBudgetBytes = budgetMB * 1024 * 1024;
            
            if (logMemoryEvents)
            {
                Debug.Log($"[MemoryManager] Memory budget updated to: {memoryBudgetMB}MB");
            }
        }
        
        /// <summary>
        /// Get total memory usage in MB
        /// </summary>
        public float GetTotalMemoryMB()
        {
            return lastTotalMemory / (1024f * 1024f);
        }
        
        /// <summary>
        /// Get managed memory usage in MB
        /// </summary>
        public float GetManagedMemoryMB()
        {
            return lastManagedMemory / (1024f * 1024f);
        }
        
        /// <summary>
        /// Get current memory pressure level
        /// </summary>
        public MemoryPressureLevel GetMemoryPressureLevel()
        {
            return currentPressureLevel;
        }
        
        /// <summary>
        /// Check if memory is within budget
        /// </summary>
        public bool IsWithinBudget()
        {
            return lastTotalMemory <= memoryBudgetBytes;
        }
        
        /// <summary>
        /// Reset memory statistics
        /// </summary>
        public void ResetMemoryStats()
        {
            peakMemoryUsage = 0;
            gcCollectionCount = 0;
            currentPressureLevel = MemoryPressureLevel.Normal;
            
            if (logMemoryEvents)
            {
                Debug.Log("[MemoryManager] Memory statistics reset");
            }
        }
    }
    
    /// <summary>
    /// Memory pressure levels
    /// </summary>
    public enum MemoryPressureLevel
    {
        Normal,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Comprehensive memory report
    /// </summary>
    [Serializable]
    public class MemoryReport
    {
        public DateTime Timestamp;
        public float TotalMemoryMB;
        public float ManagedMemoryMB;
        public float PeakMemoryMB;
        public long MemoryBudgetMB;
        public MemoryPressureLevel MemoryPressureLevel;
        public int GCCollectionCount;
        public float MemoryUtilization;
    }
}
