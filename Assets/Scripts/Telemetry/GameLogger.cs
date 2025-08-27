using System;
using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;

namespace MOBA.Telemetry
{
    /// <summary>
    /// Structured logging for FSM transitions and gameplay events.
    /// Enables debugging and metrics collection for match analysis.
    /// </summary>
    public static class GameLogger
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        private static readonly List<LogEntry> logBuffer = new List<LogEntry>();
        private static readonly object logLock = new object();

        [Serializable]
        public struct LogEntry
        {
            public uint tick;
            public LogLevel level;
            public string category;
            public string message;
            public string playerId;
            public DateTime timestamp;

            public LogEntry(uint gameTick, LogLevel logLevel, string cat, string msg, string player = "")
            {
                tick = gameTick;
                level = logLevel;
                category = cat;
                message = msg;
                playerId = player;
                timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Log an FSM state transition for debugging and metrics.
        /// </summary>
        public static void LogStateTransition(uint tick, string playerId, string fsmName, string fromState, string toState, string reason = "")
        {
            string message = $"FSM:{fsmName} {fromState} -> {toState}";
            if (!string.IsNullOrEmpty(reason))
                message += $" ({reason})";

            LogEntry entry = new LogEntry(tick, LogLevel.Info, "FSM", message, playerId);
            
            lock (logLock)
            {
                logBuffer.Add(entry);
            }

            // Also log to Unity console in development
            Debug.Log($"[T{tick}][{playerId}] {message}");
        }

        /// <summary>
        /// Log a gameplay event for metrics and analysis.
        /// </summary>
        public static void LogGameplayEvent(uint tick, string playerId, string eventType, string details = "", LogLevel level = LogLevel.Info)
        {
            LogEntry entry = new LogEntry(tick, level, eventType, details, playerId);
            
            lock (logLock)
            {
                logBuffer.Add(entry);
            }

            if (level >= LogLevel.Warning || Application.isEditor)
            {
                Debug.Log($"[T{tick}][{playerId}][{eventType}] {details}");
            }
        }

        /// <summary>
        /// Get all log entries for analysis or export.
        /// </summary>
        public static LogEntry[] GetLogs()
        {
            lock (logLock)
            {
                return logBuffer.ToArray();
            }
        }

        /// <summary>
        /// Clear the log buffer. Call after exporting logs.
        /// </summary>
        public static void ClearLogs()
        {
            lock (logLock)
            {
                logBuffer.Clear();
            }
        }
    }

    /// <summary>
    /// Metrics collection for performance analysis and balance tuning.
    /// Tracks key gameplay statistics for dashboard reporting.
    /// </summary>
    public class GameMetrics
    {
        private static GameMetrics instance;
        public static GameMetrics Instance => instance ??= new GameMetrics();

        private readonly Dictionary<string, float> metrics = new Dictionary<string, float>();
        private readonly Dictionary<string, List<float>> samples = new Dictionary<string, List<float>>();
        private readonly object metricsLock = new object();

        /// <summary>
        /// Record a single metric value.
        /// </summary>
        public void RecordMetric(string key, float value)
        {
            lock (metricsLock)
            {
                metrics[key] = value;
                
                if (!samples.ContainsKey(key))
                    samples[key] = new List<float>();
                
                samples[key].Add(value);
                
                // Keep only last 1000 samples per metric
                if (samples[key].Count > 1000)
                    samples[key].RemoveAt(0);
            }
        }

        /// <summary>
        /// Get the current value of a metric.
        /// </summary>
        public float GetMetric(string key)
        {
            lock (metricsLock)
            {
                return metrics.GetValueOrDefault(key, 0f);
            }
        }

        /// <summary>
        /// Get average of all samples for a metric.
        /// </summary>
        public float GetMetricAverage(string key)
        {
            lock (metricsLock)
            {
                if (!samples.ContainsKey(key) || samples[key].Count == 0)
                    return 0f;

                float sum = 0f;
                foreach (float sample in samples[key])
                    sum += sample;
                
                return sum / samples[key].Count;
            }
        }

        /// <summary>
        /// Export all metrics for dashboard reporting.
        /// </summary>
        public Dictionary<string, object> ExportMetrics()
        {
            lock (metricsLock)
            {
                var export = new Dictionary<string, object>();
                
                foreach (var kvp in metrics)
                {
                    export[kvp.Key + "_current"] = kvp.Value;
                    export[kvp.Key + "_average"] = GetMetricAverage(kvp.Key);
                    export[kvp.Key + "_samples"] = samples.GetValueOrDefault(kvp.Key, new List<float>()).Count;
                }
                
                return export;
            }
        }

        /// <summary>
        /// Clear all metrics. Call at match end.
        /// </summary>
        public void ClearMetrics()
        {
            lock (metricsLock)
            {
                metrics.Clear();
                samples.Clear();
            }
        }
    }
}
