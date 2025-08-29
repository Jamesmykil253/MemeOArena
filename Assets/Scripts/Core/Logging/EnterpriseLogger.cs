using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Core.Logging
{
    /// <summary>
    /// Simple enterprise-level logging system for AAA development.
    /// Provides categorized logging with levels and color-coding.
    /// </summary>
    public static class EnterpriseLogger
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }
        
        private static LogLevel minimumLevel = LogLevel.Info;
        private static bool enableFileLogging = false;
        private static Dictionary<string, LogChannelConfig> channels = new Dictionary<string, LogChannelConfig>();
        
        public struct LogChannelConfig
        {
            public LogLevel MinLevel;
            public Color Color;
            
            public LogChannelConfig(LogLevel level, Color color)
            {
                MinLevel = level;
                Color = color;
            }
        }
        
        /// <summary>
        /// Configure the logging system
        /// </summary>
        public static void Configure(LogLevel minimumLevel, bool enableFileLogging = false)
        {
            EnterpriseLogger.minimumLevel = minimumLevel;
            EnterpriseLogger.enableFileLogging = enableFileLogging;
        }
        
        /// <summary>
        /// Register a logging channel with specific configuration
        /// </summary>
        public static void RegisterChannel(string channel, LogLevel minLevel, Color color)
        {
            channels[channel] = new LogChannelConfig(minLevel, color);
        }
        
        /// <summary>
        /// Log an info message
        /// </summary>
        public static void LogInfo(string category, string channel, string message)
        {
            Log(LogLevel.Info, category, channel, message);
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarning(string category, string channel, string message)
        {
            Log(LogLevel.Warning, category, channel, message);
        }
        
        /// <summary>
        /// Log an error message
        /// </summary>
        public static void LogError(string category, string channel, string message)
        {
            Log(LogLevel.Error, category, channel, message);
        }
        
        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void LogDebug(string category, string channel, string message)
        {
            Log(LogLevel.Debug, category, channel, message);
        }
        
        private static void Log(LogLevel level, string category, string channel, string message)
        {
            // Check minimum level
            if (level < minimumLevel) return;
            
            // Check channel-specific level
            if (channels.ContainsKey(channel) && level < channels[channel].MinLevel) return;
            
            // Format message
            string formattedMessage = $"[{category}:{channel}] {message}";
            
            // Log to Unity console
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formattedMessage);
                    break;
            }
            
            // TODO: File logging if enabled
            if (enableFileLogging)
            {
                // Implementation for file logging
            }
        }
    }
}
