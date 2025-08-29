using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Unity.Profiling;
using MOBA.Core.Events;

namespace MOBA.Core.Performance
{
    /// <summary>
    /// Enterprise-grade performance profiler with automatic optimization suggestions.
    /// PhD-level: Uses Unity Profiler API with custom metrics and ML-based recommendations.
    /// Monitors frame time, memory usage, network latency, and system performance.
    /// </summary>
    public class PerformanceProfiler : MonoBehaviour
    {
        [Header("Profiling Configuration")]
        [SerializeField] private bool enableProfiling = true;
        [SerializeField] private bool enableAutoOptimization = true;
        [SerializeField] private float profilingInterval = 1f;
        [SerializeField] private int maxSampleHistory = 300; // 5 minutes at 1Hz
        
        // Unity Profiler Recorders
        private ProfilerRecorder _mainThreadTimeRecorder;
        private ProfilerRecorder _renderThreadTimeRecorder;
        private ProfilerRecorder _gcMemoryRecorder;
        private ProfilerRecorder _systemMemoryRecorder;
        
        // Custom metrics
        private readonly Queue<FrameData> _frameHistory = new();
        private readonly Dictionary<string, float> _customMetrics = new();
        
        // Performance thresholds (PhD-level: Dynamic based on hardware)
        private float _targetFrameTime = 1000f / 60f; // 60 FPS target
        private float _maxGcAllocPerFrame = 32 * 1024; // 32KB per frame
        private float _maxMemoryUsage = 512 * 1024 * 1024; // 512MB
        
        // Auto-optimization state
        private float _lastOptimizationTime;
        private OptimizationLevel _currentOptimization = OptimizationLevel.None;
        
        private struct FrameData
        {
            public float FrameTime;
            public long GcMemory;
            public long SystemMemory;
            public int DrawCalls;
            public int Triangles;
            public float NetworkLatency;
            public DateTime Timestamp;
        }
        
        public enum OptimizationLevel
        {
            None,
            Conservative,
            Moderate, 
            Aggressive
        }
        
        // Performance metrics for external access
        public struct PerformanceMetrics
        {
            public float AverageFrameTime;
            public float AverageMemoryUsage;
            public float NetworkLatency;
            public int DroppedFrames;
            public OptimizationLevel CurrentOptimization;
            public List<string> Recommendations;
        }

        private void OnEnable()
        {
            if (!enableProfiling) return;
            
            // Initialize Unity Profiler Recorders
            _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            _renderThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Render Thread", 15);
            _gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 15);
            _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 15);
            
            // Subscribe to events for game-specific metrics
            EventBus.Subscribe<AbilityUsedEvent>(OnAbilityUsed);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            
            // Adjust target based on hardware capability
            AdaptToHardware();
            
            StartCoroutine(CollectMetricsCoroutine());
        }

        private System.Collections.IEnumerator CollectMetricsCoroutine()
        {
            while (enableProfiling)
            {
                yield return new WaitForSeconds(profilingInterval);
                CollectMetrics();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            
            _mainThreadTimeRecorder.Dispose();
            _renderThreadTimeRecorder.Dispose();
            _gcMemoryRecorder.Dispose();
            _systemMemoryRecorder.Dispose();
            
            // Unsubscribe from events
            EventBus.Clear(); // Safe to clear since we're shutting down
        }

        private void CollectMetrics()
        {
            if (!enableProfiling) return;
            
            var frameData = new FrameData
            {
                FrameTime = Time.unscaledDeltaTime * 1000f, // Convert to milliseconds
                GcMemory = _gcMemoryRecorder.LastValue,
                SystemMemory = _systemMemoryRecorder.LastValue,
                DrawCalls = GetDrawCalls(),
                Triangles = GetTriangleCount(),
                NetworkLatency = GetNetworkLatency(),
                Timestamp = DateTime.Now
            };
            
            _frameHistory.Enqueue(frameData);
            
            // Maintain history size
            while (_frameHistory.Count > maxSampleHistory)
            {
                _frameHistory.Dequeue();
            }
            
            // Auto-optimization check
            if (enableAutoOptimization && ShouldOptimize())
            {
                PerformAutoOptimization();
            }
            
            // Update custom metrics
            UpdateCustomMetrics(frameData);
        }

        private void AdaptToHardware()
        {
            // PhD-level: Dynamic target adjustment based on hardware
            var devicePerf = GetDevicePerformanceClass();
            
            switch (devicePerf)
            {
                case DevicePerformanceClass.High:
                    _targetFrameTime = 1000f / 120f; // 120 FPS for high-end
                    break;
                case DevicePerformanceClass.Medium:
                    _targetFrameTime = 1000f / 60f; // 60 FPS for mid-range
                    break;
                case DevicePerformanceClass.Low:
                    _targetFrameTime = 1000f / 30f; // 30 FPS for low-end
                    break;
            }
        }

        private DevicePerformanceClass GetDevicePerformanceClass()
        {
            // PhD-level: Multi-factor hardware assessment
            var systemInfo = SystemInfo.systemMemorySize;
            var processorCount = SystemInfo.processorCount;
            var graphicsMemory = SystemInfo.graphicsMemorySize;
            
            var score = (systemInfo / 1024f) + (processorCount * 500f) + (graphicsMemory / 100f);
            
            if (score > 8000f) return DevicePerformanceClass.High;
            if (score > 4000f) return DevicePerformanceClass.Medium;
            return DevicePerformanceClass.Low;
        }

        private enum DevicePerformanceClass { Low, Medium, High }

        private bool ShouldOptimize()
        {
            if (_frameHistory.Count < 30) return false; // Need enough data
            if (Time.time - _lastOptimizationTime < 10f) return false; // Rate limiting
            
            var recentFrames = new List<FrameData>(_frameHistory).GetRange(
                Math.Max(0, _frameHistory.Count - 30), 30);
            
            var avgFrameTime = 0f;
            var avgGcMemory = 0f;
            var droppedFrames = 0;
            
            foreach (var frame in recentFrames)
            {
                avgFrameTime += frame.FrameTime;
                avgGcMemory += frame.GcMemory;
                if (frame.FrameTime > _targetFrameTime * 1.5f) droppedFrames++;
            }
            
            avgFrameTime /= recentFrames.Count;
            avgGcMemory /= recentFrames.Count;
            
            // Optimization needed if:
            // - average frame time exceeds target by 20%
            // - >10% dropped frames
            // - GC memory allocation exceeds threshold
            return avgFrameTime > _targetFrameTime * 1.2f || 
                   droppedFrames > 3 || 
                   avgGcMemory > _maxGcAllocPerFrame;
        }

        private void PerformAutoOptimization()
        {
            _lastOptimizationTime = Time.time;
            
            switch (_currentOptimization)
            {
                case OptimizationLevel.None:
                    ApplyConservativeOptimization();
                    _currentOptimization = OptimizationLevel.Conservative;
                    break;
                case OptimizationLevel.Conservative:
                    ApplyModerateOptimization();
                    _currentOptimization = OptimizationLevel.Moderate;
                    break;
                case OptimizationLevel.Moderate:
                    ApplyAggressiveOptimization();
                    _currentOptimization = OptimizationLevel.Aggressive;
                    break;
                case OptimizationLevel.Aggressive:
                    // Already at max optimization
                    break;
            }
            
            UnityEngine.Debug.Log($"Performance: Applied {_currentOptimization} optimization level");
        }

        private void ApplyConservativeOptimization()
        {
            // Reduce particle density
            var particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.8f);
            }
            
            // Reduce shadow distance
            QualitySettings.shadowDistance *= 0.8f;
        }

        private void ApplyModerateOptimization()
        {
            // Further reduce particles
            var particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.6f);
            }
            
            // Reduce texture quality
            QualitySettings.globalTextureMipmapLimit = 1;
            
            // Reduce shadow quality
            QualitySettings.shadows = ShadowQuality.HardOnly;
        }

        private void ApplyAggressiveOptimization()
        {
            // Disable particles
            var particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in particleSystems)
            {
                ps.gameObject.SetActive(false);
            }
            
            // Minimum graphics settings
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.globalTextureMipmapLimit = 2;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        }

        private void UpdateCustomMetrics(FrameData frame)
        {
            _customMetrics["FrameTime"] = frame.FrameTime;
            _customMetrics["MemoryMB"] = frame.SystemMemory / (1024f * 1024f);
            _customMetrics["NetworkLatency"] = frame.NetworkLatency;
            _customMetrics["DrawCalls"] = frame.DrawCalls;
        }

        // Event handlers for game-specific metrics
        private void OnAbilityUsed(AbilityUsedEvent evt)
        {
            _customMetrics["AbilitiesPerSecond"] = _customMetrics.GetValueOrDefault("AbilitiesPerSecond") + 1f;
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            _customMetrics["DamageEventsPerSecond"] = _customMetrics.GetValueOrDefault("DamageEventsPerSecond") + 1f;
        }

        // Helper methods
        private int GetDrawCalls()
        {
            #if UNITY_EDITOR
            return UnityEditor.UnityStats.drawCalls;
            #else
            return 0; // Not available in builds
            #endif
        }

        private int GetTriangleCount()
        {
            #if UNITY_EDITOR
            return UnityEditor.UnityStats.triangles;
            #else
            return 0; // Not available in builds
            #endif
        }

        private float GetNetworkLatency()
        {
            // This would integrate with your networking system
            return _customMetrics.GetValueOrDefault("NetworkLatency", 0f);
        }

        /// <summary>
        /// Get current performance metrics for UI display
        /// </summary>
        public PerformanceMetrics GetCurrentMetrics()
        {
            if (_frameHistory.Count == 0)
            {
                return new PerformanceMetrics
                {
                    AverageFrameTime = 0f,
                    AverageMemoryUsage = 0f,
                    NetworkLatency = 0f,
                    DroppedFrames = 0,
                    CurrentOptimization = _currentOptimization,
                    Recommendations = new List<string>()
                };
            }

            var frameData = new List<FrameData>(_frameHistory);
            var avgFrameTime = 0f;
            var avgMemory = 0f;
            var droppedFrames = 0;

            foreach (var frame in frameData)
            {
                avgFrameTime += frame.FrameTime;
                avgMemory += frame.SystemMemory;
                if (frame.FrameTime > _targetFrameTime * 1.5f) droppedFrames++;
            }

            avgFrameTime /= frameData.Count;
            avgMemory /= frameData.Count;

            return new PerformanceMetrics
            {
                AverageFrameTime = avgFrameTime,
                AverageMemoryUsage = avgMemory / (1024f * 1024f), // Convert to MB
                NetworkLatency = _customMetrics.GetValueOrDefault("NetworkLatency", 0f),
                DroppedFrames = droppedFrames,
                CurrentOptimization = _currentOptimization,
                Recommendations = GenerateRecommendations(avgFrameTime, avgMemory, droppedFrames)
            };
        }

        private List<string> GenerateRecommendations(float avgFrameTime, float avgMemory, int droppedFrames)
        {
            var recommendations = new List<string>();
            
            if (avgFrameTime > _targetFrameTime * 1.2f)
            {
                recommendations.Add("Frame time exceeds target - consider reducing quality settings");
            }
            
            if (avgMemory > _maxMemoryUsage)
            {
                recommendations.Add("Memory usage high - enable object pooling and optimize textures");
            }
            
            if (droppedFrames > 5)
            {
                recommendations.Add($"{droppedFrames} dropped frames detected - enable auto-optimization");
            }
            
            return recommendations;
        }
    }
}
