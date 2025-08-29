using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MOBA.Core.Performance
{
    /// <summary>
    /// Enterprise-grade performance profiler with real-time monitoring and reporting.
    /// Tracks frame rate, memory usage, CPU performance, and custom metrics.
    /// AAA PhD-Level: Production-quality performance analysis and optimization.
    /// </summary>
    public class PerformanceProfiler : MonoBehaviour
    {
        public static PerformanceProfiler Instance { get; private set; }
        
        [Header("Profiling Configuration")]
        [SerializeField] private bool enableRealTimeMonitoring = true;
        [SerializeField] private bool enableMemoryProfiling = true;
        [SerializeField] private bool enableCustomSampling = true;
        [SerializeField] private int historyBufferSize = 300; // 5 minutes at 60fps
        
        [Header("Performance Targets")]
        [SerializeField] private float targetFrameTime = 16.67f; // 60fps
        [SerializeField] private int warningMemoryMB = 256;
        [SerializeField] private int criticalMemoryMB = 512;
        
        [Header("Display")]
        [SerializeField] private bool showDebugUI = false;
        [SerializeField] private KeyCode toggleUIKey = KeyCode.F2;
        
        // Performance data storage
        private CircularBuffer<float> frameTimeHistory;
        private CircularBuffer<long> memoryUsageHistory;
        private Dictionary<string, CustomSample> customSamples;
        private Dictionary<string, CircularBuffer<float>> customSampleHistory;
        
        // Real-time metrics
        private float currentFPS;
        private float averageFPS;
        private float minFPS = float.MaxValue;
        private float maxFPS = float.MinValue;
        private long currentMemoryUsage;
        private float frameTimeAccumulator;
        private int frameCount;
        
        // Sampling data
        private Dictionary<string, float> activeSampleStartTimes;
        private float updateTimer;
        private const float UPDATE_INTERVAL = 0.1f; // Update 10 times per second
        
        public event Action<PerformanceReport> OnPerformanceReport;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeProfiler();
        }
        
        private void InitializeProfiler()
        {
            frameTimeHistory = new CircularBuffer<float>(historyBufferSize);
            memoryUsageHistory = new CircularBuffer<long>(historyBufferSize);
            customSamples = new Dictionary<string, CustomSample>();
            customSampleHistory = new Dictionary<string, CircularBuffer<float>>();
            activeSampleStartTimes = new Dictionary<string, float>();
            
            // Set target frame rate
            Application.targetFrameRate = Mathf.RoundToInt(1f / targetFrameTime * 1000f);
        }
        
        private void Update()
        {
            if (enableRealTimeMonitoring)
            {
                UpdateFrameRateMetrics();
                
                updateTimer += Time.unscaledDeltaTime;
                if (updateTimer >= UPDATE_INTERVAL)
                {
                    UpdateMemoryMetrics();
                    UpdateAverages();
                    updateTimer = 0f;
                }
            }
            
            if (UnityEngine.Input.GetKeyDown(toggleUIKey))
            {
                showDebugUI = !showDebugUI;
            }
        }
        
        private void UpdateFrameRateMetrics()
        {
            float frameTime = Time.unscaledDeltaTime * 1000f; // Convert to milliseconds
            frameTimeHistory.Add(frameTime);
            
            currentFPS = 1f / Time.unscaledDeltaTime;
            
            // Update min/max FPS
            if (currentFPS < minFPS) minFPS = currentFPS;
            if (currentFPS > maxFPS) maxFPS = currentFPS;
            
            // Accumulate for average calculation
            frameTimeAccumulator += frameTime;
            frameCount++;
        }
        
        private void UpdateMemoryMetrics()
        {
            if (enableMemoryProfiling)
            {
                currentMemoryUsage = (long)UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
                memoryUsageHistory.Add(currentMemoryUsage);
            }
        }
        
        private void UpdateAverages()
        {
            if (frameCount > 0)
            {
                float avgFrameTime = frameTimeAccumulator / frameCount;
                averageFPS = 1000f / avgFrameTime;
                
                frameTimeAccumulator = 0f;
                frameCount = 0;
            }
        }
        
        /// <summary>
        /// Begin a custom performance sample
        /// </summary>
        public void BeginSample(string name)
        {
            if (!enableCustomSampling) return;
            
            if (!customSamples.ContainsKey(name))
            {
                customSamples[name] = new CustomSample(name);
                customSampleHistory[name] = new CircularBuffer<float>(historyBufferSize);
            }
            
            activeSampleStartTimes[name] = Time.realtimeSinceStartup * 1000f;
            Profiler.BeginSample(name);
        }
        
        /// <summary>
        /// End a custom performance sample
        /// </summary>
        public void EndSample()
        {
            Profiler.EndSample();
        }
        
        /// <summary>
        /// End a named custom performance sample
        /// </summary>
        public void EndSample(string name)
        {
            if (!enableCustomSampling) return;
            
            if (activeSampleStartTimes.ContainsKey(name))
            {
                float elapsedTime = (Time.realtimeSinceStartup * 1000f) - activeSampleStartTimes[name];
                
                if (customSamples.ContainsKey(name))
                {
                    customSamples[name].AddSample(elapsedTime);
                    customSampleHistory[name].Add(elapsedTime);
                }
                
                activeSampleStartTimes.Remove(name);
            }
            
            Profiler.EndSample();
        }
        
        /// <summary>
        /// Generate a comprehensive performance report
        /// </summary>
        public PerformanceReport GenerateReport()
        {
            var report = new PerformanceReport
            {
                Timestamp = DateTime.Now,
                CurrentFPS = currentFPS,
                AverageFPS = averageFPS,
                MinFPS = minFPS,
                MaxFPS = maxFPS,
                CurrentMemoryMB = currentMemoryUsage / (1024f * 1024f),
                TargetFrameTime = targetFrameTime,
                CustomSamples = new Dictionary<string, CustomSample>(customSamples)
            };
            
            // Calculate performance rating
            report.PerformanceRating = CalculatePerformanceRating(report);
            
            OnPerformanceReport?.Invoke(report);
            return report;
        }
        
        private PerformanceRating CalculatePerformanceRating(PerformanceReport report)
        {
            float fpsRatio = report.AverageFPS / (1000f / targetFrameTime);
            float memoryUsageMB = report.CurrentMemoryMB;
            
            if (fpsRatio >= 0.95f && memoryUsageMB < warningMemoryMB)
                return PerformanceRating.Excellent;
            else if (fpsRatio >= 0.8f && memoryUsageMB < criticalMemoryMB)
                return PerformanceRating.Good;
            else if (fpsRatio >= 0.6f && memoryUsageMB < criticalMemoryMB)
                return PerformanceRating.Fair;
            else
                return PerformanceRating.Poor;
        }
        
        /// <summary>
        /// Set target frame rate
        /// </summary>
        public void SetTargetFrameRate(int fps)
        {
            targetFrameTime = 1000f / fps;
            Application.targetFrameRate = fps;
        }
        
        /// <summary>
        /// Get current FPS
        /// </summary>
        public float GetCurrentFPS() => currentFPS;
        
        /// <summary>
        /// Get average FPS over recent history
        /// </summary>
        public float GetAverageFPS() => averageFPS;
        
        /// <summary>
        /// Get current memory usage in MB
        /// </summary>
        public float GetCurrentMemoryMB() => currentMemoryUsage / (1024f * 1024f);
        
        /// <summary>
        /// Reset all performance metrics
        /// </summary>
        public void ResetMetrics()
        {
            minFPS = float.MaxValue;
            maxFPS = float.MinValue;
            frameTimeAccumulator = 0f;
            frameCount = 0;
            
            frameTimeHistory.Clear();
            memoryUsageHistory.Clear();
            
            foreach (var sample in customSamples.Values)
            {
                sample.Reset();
            }
            
            foreach (var history in customSampleHistory.Values)
            {
                history.Clear();
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugUI) return;
            
            GUI.backgroundColor = Color.black;
            GUI.color = Color.white;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("PERFORMANCE PROFILER", GUI.skin.label);
            GUILayout.Label($"FPS: {currentFPS:F1} (Avg: {averageFPS:F1})");
            GUILayout.Label($"Frame Time: {Time.unscaledDeltaTime * 1000f:F2}ms");
            GUILayout.Label($"Memory: {GetCurrentMemoryMB():F1}MB");
            
            var rating = CalculatePerformanceRating(GenerateReport());
            Color ratingColor = rating switch
            {
                PerformanceRating.Excellent => Color.green,
                PerformanceRating.Good => Color.yellow,
                PerformanceRating.Fair => new Color(1f, 0.5f, 0f), // Orange
                PerformanceRating.Poor => Color.red,
                _ => Color.white
            };
            
            GUI.color = ratingColor;
            GUILayout.Label($"Rating: {rating}");
            GUI.color = Color.white;
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
    
    /// <summary>
    /// Custom performance sample data
    /// </summary>
    [Serializable]
    public class CustomSample
    {
        public string Name { get; private set; }
        public float TotalTime { get; private set; }
        public int SampleCount { get; private set; }
        public float AverageTime => SampleCount > 0 ? TotalTime / SampleCount : 0f;
        public float MinTime { get; private set; } = float.MaxValue;
        public float MaxTime { get; private set; } = float.MinValue;
        
        public CustomSample(string name)
        {
            Name = name;
        }
        
        public void AddSample(float time)
        {
            TotalTime += time;
            SampleCount++;
            
            if (time < MinTime) MinTime = time;
            if (time > MaxTime) MaxTime = time;
        }
        
        public void Reset()
        {
            TotalTime = 0f;
            SampleCount = 0;
            MinTime = float.MaxValue;
            MaxTime = float.MinValue;
        }
    }
    
    /// <summary>
    /// Comprehensive performance report
    /// </summary>
    [Serializable]
    public class PerformanceReport
    {
        public DateTime Timestamp;
        public float CurrentFPS;
        public float AverageFPS;
        public float MinFPS;
        public float MaxFPS;
        public float CurrentMemoryMB;
        public float TargetFrameTime;
        public PerformanceRating PerformanceRating;
        public Dictionary<string, CustomSample> CustomSamples;
    }
    
    /// <summary>
    /// Performance rating enumeration
    /// </summary>
    public enum PerformanceRating
    {
        Excellent,
        Good,
        Fair,
        Poor
    }
    
    /// <summary>
    /// Circular buffer for efficient history storage
    /// </summary>
    public class CircularBuffer<T>
    {
        private T[] buffer;
        private int head;
        private int count;
        private int capacity;
        
        public CircularBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
        }
        
        public void Add(T item)
        {
            buffer[head] = item;
            head = (head + 1) % capacity;
            
            if (count < capacity)
                count++;
        }
        
        public void Clear()
        {
            head = 0;
            count = 0;
        }
        
        public T[] GetData()
        {
            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = buffer[(head - count + i + capacity) % capacity];
            }
            return result;
        }
    }
}
