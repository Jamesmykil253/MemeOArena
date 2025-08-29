using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using MOBA.Core.Events;

namespace MOBA.Core.Events
{
    /// <summary>
    /// Enterprise event bus system for decoupled game architecture.
    /// Provides asynchronous event publishing and subscription.
    /// </summary>
    public static class EventBus
    {
        private static Dictionary<Type, List<object>> subscribers = new Dictionary<Type, List<object>>();
        private static Queue<IGameEvent> eventQueue = new Queue<IGameEvent>();
        private static bool isProcessing = false;
        
        /// <summary>
        /// Subscribe to events of type T
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            
            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new List<object>();
            }
            
            subscribers[eventType].Add(handler);
        }
        
        /// <summary>
        /// Unsubscribe from events of type T
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            
            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Remove(handler);
                
                if (subscribers[eventType].Count == 0)
                {
                    subscribers.Remove(eventType);
                }
            }
        }
        
        /// <summary>
        /// Publish an event immediately
        /// </summary>
        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var eventType = typeof(T);
            
            if (subscribers.ContainsKey(eventType))
            {
                foreach (var subscriber in subscribers[eventType])
                {
                    if (subscriber is Action<T> action)
                    {
                        try
                        {
                            action.Invoke(gameEvent);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[EventBus] Error handling event {eventType.Name}: {e.Message}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Queue an event for asynchronous processing
        /// </summary>
        public static void PublishAsync<T>(T gameEvent) where T : IGameEvent
        {
            eventQueue.Enqueue(gameEvent);
        }
        
        /// <summary>
        /// Process all queued events
        /// </summary>
        public static void ProcessQueuedEvents()
        {
            if (isProcessing) return;
            
            isProcessing = true;
            
            try
            {
                while (eventQueue.Count > 0)
                {
                    var gameEvent = eventQueue.Dequeue();
                    var eventType = gameEvent.GetType();
                    
                    if (subscribers.ContainsKey(eventType))
                    {
                        foreach (var subscriber in subscribers[eventType])
                        {
                            try
                            {
                                var method = subscriber.GetType().GetMethod("Invoke");
                                method?.Invoke(subscriber, new object[] { gameEvent });
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"[EventBus] Error processing queued event {eventType.Name}: {e.Message}");
                            }
                        }
                    }
                }
            }
            finally
            {
                isProcessing = false;
            }
        }
        
        /// <summary>
        /// Clear all subscribers and queued events
        /// </summary>
        public static void Clear()
        {
            subscribers.Clear();
            eventQueue.Clear();
        }
        
        /// <summary>
        /// Get the number of subscribers for an event type
        /// </summary>
        public static int GetSubscriberCount<T>() where T : IGameEvent
        {
            var eventType = typeof(T);
            return subscribers.ContainsKey(eventType) ? subscribers[eventType].Count : 0;
        }
        
        /// <summary>
        /// Get the number of queued events
        /// </summary>
        public static int GetQueuedEventCount()
        {
            return eventQueue.Count;
        }
    }
}
