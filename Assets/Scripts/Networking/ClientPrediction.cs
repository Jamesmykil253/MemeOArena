using System;
using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;
using MOBA.Telemetry;

namespace MOBA.Networking
{
    /// <summary>
    /// Manages client prediction for smooth gameplay feel.
    /// Handles input buffering, state interpolation, and rollback correction.
    /// </summary>
    public class ClientPrediction : MonoBehaviour
    {
        [Header("Prediction Settings")]
        [SerializeField] private int maxPredictionFrames = 60; // 1.2 seconds at 50Hz
        [SerializeField] private float correctionThreshold = 0.1f; // Minimum error for correction
        [SerializeField] private float smoothingSpeed = 10f; // Speed of error correction
        
        // Prediction state
        private readonly Queue<PredictionFrame> predictionBuffer = new Queue<PredictionFrame>();
        private readonly Dictionary<uint, GameSnapshot> confirmedStates = new Dictionary<uint, GameSnapshot>();
        
        private Vector3 positionError = Vector3.zero;
        private Vector3 velocityError = Vector3.zero;
        private bool isReconciling = false;
        
        // Events
        public static event Action<Vector3> OnPositionCorrection;
        public static event Action<Vector3> OnVelocityCorrection;
        
        private NetworkManager networkManager;
        private TickManager tickManager;
        
        public bool IsReconciling => isReconciling;
        public Vector3 PositionError => positionError;
        public Vector3 VelocityError => velocityError;
        
        private void Awake()
        {
            networkManager = GetComponent<NetworkManager>();
            tickManager = FindFirstObjectByType<TickManager>();
        }
        
        private void Start()
        {
            // Subscribe to network events
            NetworkManager.OnServerSnapshot += OnServerSnapshot;
            TickManager.OnTick += OnTick;
            
            GameLogger.LogGameplayEvent(0, "CLIENT", "PREDICTION_INIT", "Client prediction initialized");
        }
        
        private void OnTick(uint tick)
        {
            if (networkManager.IsServer) return;
            
            // Create prediction frame for this tick
            CreatePredictionFrame(tick);
            
            // Clean up old prediction frames
            CleanupOldFrames();
        }
        
        private void Update()
        {
            if (networkManager.IsServer) return;
            
            // Apply error correction smoothing
            if (isReconciling)
            {
                ApplyErrorCorrection();
            }
        }
        
        /// <summary>
        /// Create a prediction frame capturing current input and predicted state
        /// </summary>
        public void CreatePredictionFrame(uint tick)
        {
            // Get current input state
            InputCmd currentInput = GetCurrentInput(tick);
            
            // Predict next state based on current input
            GameSnapshot predictedState = PredictNextState(currentInput, tick);
            
            // Store prediction frame
            PredictionFrame frame = new PredictionFrame
            {
                tick = tick,
                input = currentInput,
                predictedState = predictedState,
                timestamp = Time.time
            };
            
            predictionBuffer.Enqueue(frame);
            
            // Limit buffer size
            while (predictionBuffer.Count > maxPredictionFrames)
            {
                predictionBuffer.Dequeue();
            }
        }
        
        /// <summary>
        /// Apply input to predict the next game state
        /// </summary>
        public GameSnapshot PredictNextState(InputCmd input, uint tick)
        {
            GameSnapshot newState = new GameSnapshot();
            
            // Get base state (latest confirmed or previous prediction)
            GameSnapshot baseState = GetBaseState(tick);
            if (baseState != null)
            {
                newState = new GameSnapshot(baseState);
            }
            
            // Apply input prediction
            ApplyInputPrediction(newState, input);
            
            newState.tick = tick;
            return newState;
        }
        
        private void OnServerSnapshot(Snapshot snapshot)
        {
            // Store confirmed state
            confirmedStates[snapshot.tick] = new GameSnapshot(snapshot);
            
            // Check for prediction errors
            CheckPredictionAccuracy(snapshot);
            
            // Perform reconciliation if needed
            if (ShouldReconcile(snapshot))
            {
                PerformReconciliation(snapshot);
            }
            
            GameLogger.LogGameplayEvent(snapshot.tick, "CLIENT", "SNAPSHOT_RECV", 
                $"Seq:{snapshot.lastProcessedSeq}");
        }
        
        private void CheckPredictionAccuracy(Snapshot snapshot)
        {
            // Find corresponding prediction frame
            PredictionFrame matchingFrame = FindPredictionFrame(snapshot.tick);
            if (matchingFrame == null) return;
            
            // Calculate prediction error
            Vector3 posError = snapshot.position - matchingFrame.predictedState.position;
            Vector3 velError = snapshot.velocity - matchingFrame.predictedState.velocity;
            
            float posErrorMag = posError.magnitude;
            float velErrorMag = velError.magnitude;
            
            // Log prediction accuracy
            GameMetrics.Instance.RecordMetric("prediction_position_error", posErrorMag);
            GameMetrics.Instance.RecordMetric("prediction_velocity_error", velErrorMag);
            
            if (posErrorMag > correctionThreshold)
            {
                GameLogger.LogGameplayEvent(snapshot.tick, "CLIENT", "PREDICTION_ERROR", 
                    $"PosError:{posErrorMag:F3} VelError:{velErrorMag:F3}");
            }
        }
        
        private bool ShouldReconcile(Snapshot snapshot)
        {
            PredictionFrame matchingFrame = FindPredictionFrame(snapshot.tick);
            if (matchingFrame == null) return false;
            
            Vector3 posError = snapshot.position - matchingFrame.predictedState.position;
            return posError.magnitude > correctionThreshold;
        }
        
        private void PerformReconciliation(Snapshot snapshot)
        {
            isReconciling = true;
            
            // Calculate correction needed
            PredictionFrame matchingFrame = FindPredictionFrame(snapshot.tick);
            if (matchingFrame != null)
            {
                positionError = snapshot.position - matchingFrame.predictedState.position;
                velocityError = snapshot.velocity - matchingFrame.predictedState.velocity;
            }
            
            // Rollback and replay from server state
            RollbackAndReplay(snapshot);
            
            GameLogger.LogGameplayEvent(snapshot.tick, "CLIENT", "RECONCILE_START", 
                $"Error:{positionError.magnitude:F3}");
            
            GameMetrics.Instance.RecordMetric("client_reconciliations", 1);
        }
        
        private void RollbackAndReplay(Snapshot snapshot)
        {
            // Set current state to authoritative server state
            GameSnapshot authorityState = new GameSnapshot(snapshot);
            
            // Find all prediction frames after this snapshot
            List<PredictionFrame> framesToReplay = new List<PredictionFrame>();
            foreach (var frame in predictionBuffer)
            {
                if (frame.tick > snapshot.tick)
                {
                    framesToReplay.Add(frame);
                }
            }
            
            // Sort frames by tick
            framesToReplay.Sort((a, b) => a.tick.CompareTo(b.tick));
            
            // Replay inputs on top of authority state
            GameSnapshot replayState = new GameSnapshot(authorityState);
            foreach (var frame in framesToReplay)
            {
                ApplyInputPrediction(replayState, frame.input);
                frame.predictedState = new GameSnapshot(replayState);
            }
            
            GameLogger.LogGameplayEvent(snapshot.tick, "CLIENT", "ROLLBACK_COMPLETE", 
                $"Replayed {framesToReplay.Count} frames");
        }
        
        private void ApplyErrorCorrection()
        {
            // Smoothly correct position and velocity errors
            float deltaTime = Time.deltaTime;
            float correctionAmount = smoothingSpeed * deltaTime;
            
            Vector3 posCorrection = Vector3.MoveTowards(Vector3.zero, positionError, correctionAmount);
            Vector3 velCorrection = Vector3.MoveTowards(Vector3.zero, velocityError, correctionAmount);
            
            // Apply corrections
            positionError -= posCorrection;
            velocityError -= velCorrection;
            
            // Notify listeners
            if (posCorrection.magnitude > 0.001f)
            {
                OnPositionCorrection?.Invoke(posCorrection);
            }
            if (velCorrection.magnitude > 0.001f)
            {
                OnVelocityCorrection?.Invoke(velCorrection);
            }
            
            // Check if reconciliation is complete
            if (positionError.magnitude < 0.01f && velocityError.magnitude < 0.01f)
            {
                isReconciling = false;
                positionError = Vector3.zero;
                velocityError = Vector3.zero;
                
                GameLogger.LogGameplayEvent(tickManager.CurrentTick, "CLIENT", "RECONCILE_COMPLETE", 
                    "Error correction finished");
            }
        }
        
        private InputCmd GetCurrentInput(uint tick)
        {
            // Get current input state for prediction
            // This would integrate with the input system
            return new InputCmd(tick, Vector2.zero, false, false, false, false, false);
        }
        
        private GameSnapshot GetBaseState(uint tick)
        {
            // Get the most recent confirmed state before this tick
            GameSnapshot baseState = null;
            uint latestTick = 0;
            
            foreach (var kvp in confirmedStates)
            {
                if (kvp.Key <= tick && kvp.Key > latestTick)
                {
                    latestTick = kvp.Key;
                    baseState = kvp.Value;
                }
            }
            
            return baseState;
        }
        
        private void ApplyInputPrediction(GameSnapshot state, InputCmd input)
        {
            // Apply input to predict state changes
            // This would integrate with game controllers
            
            float dt = tickManager.TickInterval;
            
            // Simple movement prediction
            Vector3 moveInput = new Vector3(input.moveInput.x, 0f, input.moveInput.y);
            state.velocity = moveInput * 5f; // Assume move speed of 5
            state.position += state.velocity * dt;
            
            // Jump prediction
            if (input.jumpPressed)
            {
                state.velocity.y = 10f; // Jump velocity
            }
            
            // Apply gravity
            state.velocity.y += -20f * dt; // Gravity
            
            // Ground clamping
            if (state.position.y < 0f)
            {
                state.position.y = 0f;
                state.velocity.y = 0f;
            }
        }
        
        private PredictionFrame FindPredictionFrame(uint tick)
        {
            foreach (var frame in predictionBuffer)
            {
                if (frame.tick == tick)
                    return frame;
            }
            return null;
        }
        
        private void CleanupOldFrames()
        {
            uint currentTick = tickManager.CurrentTick;
            uint cleanupThreshold = (uint)maxPredictionFrames;
            
            // Remove old prediction frames
            while (predictionBuffer.Count > 0 && 
                   predictionBuffer.Peek().tick < currentTick - cleanupThreshold)
            {
                predictionBuffer.Dequeue();
            }
            
            // Remove old confirmed states
            List<uint> ticksToRemove = new List<uint>();
            foreach (uint tick in confirmedStates.Keys)
            {
                if (tick < currentTick - cleanupThreshold)
                {
                    ticksToRemove.Add(tick);
                }
            }
            
            foreach (uint tick in ticksToRemove)
            {
                confirmedStates.Remove(tick);
            }
        }
        
        private void OnDestroy()
        {
            NetworkManager.OnServerSnapshot -= OnServerSnapshot;
            if (tickManager != null)
            {
                TickManager.OnTick -= OnTick;
            }
        }
    }
    
    /// <summary>
    /// Represents a single frame of client prediction data
    /// </summary>
    [Serializable]
    public class PredictionFrame
    {
        public uint tick;
        public InputCmd input;
        public GameSnapshot predictedState;
        public float timestamp;
    }
}
