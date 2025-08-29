using System;

namespace MOBA.Core.Events
{
    /// <summary>
    /// Base interface for all game events in the event-driven architecture.
    /// Provides contract for event identification, timing, and data payload.
    /// AAA PhD-Level: Foundation for decoupled, event-driven game systems.
    /// </summary>
    public interface IGameEvent
    {
        /// <summary>
        /// Unique identifier for the event type
        /// </summary>
        string EventType { get; }
        
        /// <summary>
        /// Timestamp when the event was created
        /// </summary>
        float Timestamp { get; }
        
        /// <summary>
        /// Event-specific data payload
        /// </summary>
        object Data { get; }
        
        /// <summary>
        /// Priority level for event processing
        /// </summary>
        EventPriority Priority { get; }
        
        /// <summary>
        /// Whether this event should be logged for debugging
        /// </summary>
        bool ShouldLog { get; }
    }
    
    /// <summary>
    /// Event priority levels for processing order
    /// </summary>
    public enum EventPriority
    {
        Low = 0,
        Normal = 1, 
        High = 2,
        Critical = 3
    }
    
    /// <summary>
    /// Base implementation of IGameEvent with common functionality
    /// </summary>
    public abstract class GameEventBase : IGameEvent
    {
        public abstract string EventType { get; }
        public float Timestamp { get; private set; }
        public abstract object Data { get; }
        public virtual EventPriority Priority => EventPriority.Normal;
        public virtual bool ShouldLog => true;
        
        protected GameEventBase()
        {
            Timestamp = UnityEngine.Time.time;
        }
    }
}
