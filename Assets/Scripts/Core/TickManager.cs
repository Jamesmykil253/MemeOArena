using System;
using System.Collections.Generic;
using UnityEngine;
using MOBA.Telemetry;
using MOBA.Networking;

namespace MOBA.Core
{
    /// <summary>
    /// Manages deterministic fixed-timestep simulation.
    /// Ensures consistent behavior across clients and enables deterministic replay.
    /// </summary>
    public class TickManager : MonoBehaviour
    {
        [Header("Tick Configuration")]
        [SerializeField] private float tickRate = 50f; // 50Hz = 20ms per tick
        [SerializeField] private int maxTicksPerFrame = 4; // Prevent spiral of death
        
        private float tickInterval;
        private float accumulator;
        private uint currentTick;
        private bool isRunning;
        
        // Input handling
        private readonly Queue<InputCmd> inputQueue = new Queue<InputCmd>();
        private readonly Dictionary<string, InputCmd> lastInputs = new Dictionary<string, InputCmd>();
        
        // Events
        public static event Action<uint> OnTick;
        public static event Action<float> OnFixedUpdate;
        public static event Action<Snapshot> OnSnapshot;

        public uint CurrentTick => currentTick;
        public float TickInterval => tickInterval;
        public bool IsRunning => isRunning;

        private void Awake()
        {
            tickInterval = 1f / tickRate;
            
            // Set Unity's fixed timestep to match our tick rate
            Time.fixedDeltaTime = tickInterval;
            
            GameLogger.LogGameplayEvent(0, "SYSTEM", "TICK_MANAGER", $"Initialized with {tickRate}Hz tick rate");
        }

        private void Start()
        {
            StartSimulation();
        }

        /// <summary>
        /// Start the deterministic simulation tick loop.
        /// </summary>
        public void StartSimulation()
        {
            isRunning = true;
            accumulator = 0f;
            GameLogger.LogGameplayEvent(currentTick, "SYSTEM", "SIMULATION", "Started");
        }

        /// <summary>
        /// Stop the simulation. Used for pause/menu states.
        /// </summary>
        public void StopSimulation()
        {
            isRunning = false;
            GameLogger.LogGameplayEvent(currentTick, "SYSTEM", "SIMULATION", "Stopped");
        }

        /// <summary>
        /// Queue an input command to be processed on the next tick.
        /// </summary>
        public void QueueInput(InputCmd input)
        {
            inputQueue.Enqueue(input);
            lastInputs[input.sequenceNumber.ToString()] = input;
        }

        /// <summary>
        /// Get the last input for a given sequence number.
        /// Used for client prediction rollback.
        /// </summary>
        public InputCmd GetLastInput(uint sequenceNumber)
        {
            string key = sequenceNumber.ToString();
            return lastInputs.GetValueOrDefault(key, default(InputCmd));
        }

        private void Update()
        {
            if (!isRunning) return;

            accumulator += Time.deltaTime;
            
            int ticksThisFrame = 0;
            
            // Process accumulated time in fixed-size chunks
            while (accumulator >= tickInterval && ticksThisFrame < maxTicksPerFrame)
            {
                ProcessTick();
                accumulator -= tickInterval;
                ticksThisFrame++;
            }
            
            // Track performance metrics
            if (ticksThisFrame > 1)
            {
                GameMetrics.Instance.RecordMetric("ticks_per_frame", ticksThisFrame);
                if (ticksThisFrame >= maxTicksPerFrame)
                {
                    GameLogger.LogGameplayEvent(currentTick, "SYSTEM", "PERFORMANCE", 
                        $"Hit max ticks per frame: {maxTicksPerFrame}", GameLogger.LogLevel.Warning);
                }
            }
        }

        private void ProcessTick()
        {
            currentTick++;
            
            // Process all queued inputs for this tick
            ProcessInputs();
            
            // Fire tick event for all game systems
            OnTick?.Invoke(currentTick);
            
            // Fire fixed update event  
            OnFixedUpdate?.Invoke(tickInterval);
            
            // Record tick timing for metrics
            GameMetrics.Instance.RecordMetric("current_tick", currentTick);
            GameMetrics.Instance.RecordMetric("tick_interval", tickInterval);
        }

        private void ProcessInputs()
        {
            while (inputQueue.Count > 0)
            {
                InputCmd input = inputQueue.Dequeue();
                
                // Log input for debugging
                if (input.moveInput != Vector2.zero || input.jumpPressed)
                {
                    GameLogger.LogGameplayEvent(currentTick, input.sequenceNumber.ToString(), 
                        "INPUT", $"Move:{input.moveInput} Jump:{input.jumpPressed}");
                }
                
                // Input will be consumed by controllers that subscribe to the tick event
            }
        }

        /// <summary>
        /// Create and broadcast a snapshot of the current game state.
        /// Called by the server to send authoritative state to clients.
        /// </summary>
        public void BroadcastSnapshot(Vector3 position, Vector3 velocity, float ultimateEnergy, 
                                     int carriedPoints, int currentHP, byte locState, byte abState, byte scoreState)
        {
            uint lastSeq = inputQueue.Count > 0 ? inputQueue.Peek().sequenceNumber : 0;
            
            Snapshot snapshot = new Snapshot(lastSeq, currentTick, position, velocity, 
                                           ultimateEnergy, carriedPoints, currentHP,
                                           locState, abState, scoreState);
            
            OnSnapshot?.Invoke(snapshot);
            
            GameLogger.LogGameplayEvent(currentTick, "SYSTEM", "SNAPSHOT", 
                $"Broadcast: Pos={position:F2} Energy={ultimateEnergy:F1}");
        }

        /// <summary>
        /// Reset simulation to a specific tick. Used for rollback networking.
        /// </summary>
        public void ResetToTick(uint tick)
        {
            if (tick > currentTick)
            {
                GameLogger.LogGameplayEvent(currentTick, "SYSTEM", "ROLLBACK", 
                    $"Cannot rollback to future tick {tick}", GameLogger.LogLevel.Error);
                return;
            }
            
            currentTick = tick;
            accumulator = 0f;
            
            GameLogger.LogGameplayEvent(currentTick, "SYSTEM", "ROLLBACK", $"Reset to tick {tick}");
        }

        private void OnDestroy()
        {
            StopSimulation();
        }
    }
}
