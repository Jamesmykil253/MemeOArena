using System;
using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;
using MOBA.Data;
using MOBA.Telemetry;

namespace MOBA.Networking
{
    /// <summary>
    /// Manages client-server networking with client prediction and server reconciliation.
    /// Handles input buffering, snapshot processing, and rollback networking.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private bool isServer = false;
        [SerializeField] private float snapshotRate = 20f; // 20Hz snapshots
        [SerializeField] private int maxBufferedInputs = 120; // 2.4 seconds at 50Hz
        [SerializeField] private float interpolationDelay = 0.1f; // 100ms interpolation buffer
        
        // Server state
        private readonly Dictionary<string, PlayerNetworkState> serverPlayerStates = 
            new Dictionary<string, PlayerNetworkState>();
        
        // Client state
        private readonly Queue<InputCmd> unackedInputs = new Queue<InputCmd>();
        private readonly Dictionary<uint, GameSnapshot> serverSnapshots = 
            new Dictionary<uint, GameSnapshot>();
        private uint lastAckedInput = 0;
        private uint inputSequence = 0;
        
        // Prediction state
        private GameSnapshot predictedState;
        private GameSnapshot authorityState;
        private bool needsReconciliation = false;
        
        // Events
        public static event Action<Snapshot> OnServerSnapshot;
        public static event Action<GameEvent> OnGameEvent;
        public static event Action<string, InputCmd> OnInputReceived; // playerId, input
        
        private float snapshotTimer;
        private TickManager tickManager;
        
        public bool IsServer => isServer;
        public uint InputSequence => inputSequence;
        public float InterpolationDelay => interpolationDelay;
        
        private void Awake()
        {
            tickManager = FindFirstObjectByType<TickManager>();
            if (tickManager == null)
            {
                GameLogger.LogGameplayEvent(0, "SYSTEM", "ERROR", "NetworkManager requires TickManager");
            }
        }
        
        private void Start()
        {
            // Subscribe to tick events
            if (tickManager != null)
            {
                TickManager.OnTick += OnTick;
                TickManager.OnSnapshot += OnSnapshotReceived;
            }
            
            InitializeNetworking();
            
            GameLogger.LogGameplayEvent(0, "NETWORK", "INIT", 
                $"Initialized as {(isServer ? "Server" : "Client")}");
        }
        
        private void Update()
        {
            if (isServer)
            {
                UpdateServer();
            }
            else
            {
                UpdateClient();
            }
        }
        
        private void InitializeNetworking()
        {
            if (isServer)
            {
                // Server initialization
                snapshotTimer = 1f / snapshotRate;
                GameMetrics.Instance.RecordMetric("server_snapshot_rate", snapshotRate);
            }
            else
            {
                // Client initialization  
                predictedState = new GameSnapshot();
                authorityState = new GameSnapshot();
                
                GameMetrics.Instance.RecordMetric("client_interpolation_delay", interpolationDelay);
            }
        }
        
        private void OnTick(uint tick)
        {
            if (isServer)
            {
                ProcessServerTick(tick);
            }
            else
            {
                ProcessClientTick(tick);
            }
        }
        
        private void UpdateServer()
        {
            snapshotTimer -= Time.deltaTime;
            if (snapshotTimer <= 0f)
            {
                BroadcastSnapshots();
                snapshotTimer = 1f / snapshotRate;
            }
        }
        
        private void UpdateClient()
        {
            if (needsReconciliation)
            {
                PerformReconciliation();
                needsReconciliation = false;
            }
            
            // Clean up old snapshots
            CleanupOldSnapshots();
        }
        
        private void ProcessServerTick(uint tick)
        {
            // Process all received inputs for this tick
            foreach (var playerState in serverPlayerStates.Values)
            {
                if (playerState.HasUnprocessedInput())
                {
                    InputCmd input = playerState.GetNextInput();
                    ProcessInputOnServer(playerState.PlayerId, input, tick);
                }
            }
        }
        
        private void ProcessClientTick(uint tick)
        {
            // Client prediction - apply inputs immediately
            if (unackedInputs.Count > 0)
            {
                // Update predicted state based on inputs
                UpdatePredictedState(tick);
            }
        }
        
        /// <summary>
        /// Send input from client to server with prediction
        /// </summary>
        public void SendInput(Vector2 moveInput, bool jump, bool ability1, bool ability2, 
                             bool ultimate, bool scoring)
        {
            if (isServer) return;
            
            inputSequence++;
            InputCmd input = new InputCmd(inputSequence, moveInput, jump, ability1, ability2, ultimate, scoring);
            
            // Store for potential rollback
            unackedInputs.Enqueue(input);
            if (unackedInputs.Count > maxBufferedInputs)
            {
                unackedInputs.Dequeue();
                GameLogger.LogGameplayEvent(tickManager.CurrentTick, "CLIENT", "INPUT_OVERFLOW", 
                    "Dropped oldest unacked input");
            }
            
            // Apply immediately for prediction
            ApplyInputLocally(input);
            
            // Send to server (in real implementation this would be network send)
            SimulateSendToServer(input);
            
            GameLogger.LogGameplayEvent(tickManager.CurrentTick, "CLIENT", "INPUT_SEND", 
                $"Seq:{inputSequence} Move:{moveInput}");
        }
        
        /// <summary>
        /// Process input received by server from client
        /// </summary>
        public void ReceiveInput(string playerId, InputCmd input)
        {
            if (!isServer) return;
            
            if (!serverPlayerStates.ContainsKey(playerId))
            {
                serverPlayerStates[playerId] = new PlayerNetworkState(playerId);
            }
            
            serverPlayerStates[playerId].AddInput(input);
            OnInputReceived?.Invoke(playerId, input);
            
            GameLogger.LogGameplayEvent(tickManager.CurrentTick, playerId, "INPUT_RECV", 
                $"Seq:{input.sequenceNumber}");
        }
        
        private void ProcessInputOnServer(string playerId, InputCmd input, uint tick)
        {
            // Apply input to authoritative game state
            // This would integrate with the actual game controllers
            
            var playerState = serverPlayerStates[playerId];
            playerState.LastProcessedInput = input.sequenceNumber;
            
            GameLogger.LogGameplayEvent(tick, playerId, "INPUT_PROCESS", 
                $"Processed seq:{input.sequenceNumber}");
        }
        
        private void ApplyInputLocally(InputCmd input)
        {
            // Apply input to predicted state
            // This would integrate with the local controllers
            
            GameMetrics.Instance.RecordMetric("client_predicted_inputs", 1);
        }
        
        private void UpdatePredictedState(uint tick)
        {
            // Update predicted state based on queued inputs
            // This is where client prediction happens
        }
        
        private void OnSnapshotReceived(Snapshot snapshot)
        {
            if (isServer) return;
            
            // Store snapshot for interpolation
            uint tick = snapshot.tick;
            serverSnapshots[tick] = new GameSnapshot(snapshot);
            
            // Check if we need reconciliation
            if (snapshot.lastProcessedSeq > lastAckedInput)
            {
                lastAckedInput = snapshot.lastProcessedSeq;
                authorityState = serverSnapshots[tick];
                needsReconciliation = true;
                
                GameLogger.LogGameplayEvent(tick, "CLIENT", "RECONCILE_NEEDED", 
                    $"LastAcked:{lastAckedInput}");
            }
            
            OnServerSnapshot?.Invoke(snapshot);
        }
        
        private void PerformReconciliation()
        {
            // Remove acknowledged inputs
            while (unackedInputs.Count > 0 && unackedInputs.Peek().sequenceNumber <= lastAckedInput)
            {
                unackedInputs.Dequeue();
            }
            
            // Rollback to authority state
            predictedState = new GameSnapshot(authorityState);
            
            // Replay unacked inputs
            foreach (var input in unackedInputs)
            {
                ApplyInputToState(predictedState, input);
            }
            
            GameLogger.LogGameplayEvent(tickManager.CurrentTick, "CLIENT", "RECONCILE_COMPLETE", 
                $"Replayed {unackedInputs.Count} inputs");
            
            GameMetrics.Instance.RecordMetric("client_reconciliations", 1);
        }
        
        private void ApplyInputToState(GameSnapshot state, InputCmd input)
        {
            // Apply input to the given state snapshot
            // This would update position, velocity, etc.
        }
        
        private void BroadcastSnapshots()
        {
            // Create and broadcast snapshots to all connected clients
            foreach (var kvp in serverPlayerStates)
            {
                string playerId = kvp.Key;
                var playerState = kvp.Value;
                
                // Create snapshot from current server state
                Snapshot snapshot = CreatePlayerSnapshot(playerId, playerState);
                
                // Broadcast to client (in real implementation)
                SimulateSendToClient(playerId, snapshot);
            }
            
            GameMetrics.Instance.RecordMetric("server_snapshots_sent", serverPlayerStates.Count);
        }
        
        private Snapshot CreatePlayerSnapshot(string playerId, PlayerNetworkState playerState)
        {
            // Create snapshot from current game state
            // This would gather data from actual game controllers
            
            return new Snapshot(
                playerState.LastProcessedInput,
                tickManager.CurrentTick,
                Vector3.zero, // position from game state
                Vector3.zero, // velocity from game state
                0f, // ultimate energy
                0, // carried points
                100, // current HP
                0, // locomotion state
                0, // ability state
                0  // scoring state
            );
        }
        
        private void SimulateSendToServer(InputCmd input)
        {
            // In real implementation, this would send over network
            // For now, simulate server receiving input
            ReceiveInput("localPlayer", input);
        }
        
        private void SimulateSendToClient(string playerId, Snapshot snapshot)
        {
            // In real implementation, this would send over network
            // For now, simulate client receiving snapshot
            if (!isServer) // Only if we're also simulating client
            {
                OnSnapshotReceived(snapshot);
            }
        }
        
        private void CleanupOldSnapshots()
        {
            uint currentTick = tickManager.CurrentTick;
            uint cleanupThreshold = (uint)(snapshotRate * 2f); // Keep 2 seconds of snapshots
            
            List<uint> toRemove = new List<uint>();
            foreach (uint tick in serverSnapshots.Keys)
            {
                if (currentTick > tick + cleanupThreshold)
                {
                    toRemove.Add(tick);
                }
            }
            
            foreach (uint tick in toRemove)
            {
                serverSnapshots.Remove(tick);
            }
        }
        
        /// <summary>
        /// Broadcast a game event to all clients
        /// </summary>
        public void BroadcastGameEvent(GameEvent gameEvent)
        {
            if (!isServer) return;
            
            OnGameEvent?.Invoke(gameEvent);
            
            GameLogger.LogGameplayEvent(gameEvent.tick, gameEvent.playerId, "GAME_EVENT", 
                $"Type:{gameEvent.eventType} Value:{gameEvent.value}");
        }
        
        private void OnDestroy()
        {
            if (tickManager != null)
            {
                TickManager.OnTick -= OnTick;
                TickManager.OnSnapshot -= OnSnapshotReceived;
            }
        }
    }
    
    /// <summary>
    /// Server-side representation of a player's network state
    /// </summary>
    [Serializable]
    public class PlayerNetworkState
    {
        public string PlayerId { get; private set; }
        public uint LastProcessedInput { get; set; }
        
        private readonly Queue<InputCmd> inputBuffer = new Queue<InputCmd>();
        
        public PlayerNetworkState(string playerId)
        {
            PlayerId = playerId;
            LastProcessedInput = 0;
        }
        
        public void AddInput(InputCmd input)
        {
            inputBuffer.Enqueue(input);
        }
        
        public bool HasUnprocessedInput()
        {
            return inputBuffer.Count > 0;
        }
        
        public InputCmd GetNextInput()
        {
            return inputBuffer.Count > 0 ? inputBuffer.Dequeue() : default(InputCmd);
        }
    }
    
    /// <summary>
    /// Complete game state snapshot for rollback networking
    /// </summary>
    [Serializable]
    public class GameSnapshot
    {
        public Vector3 position;
        public Vector3 velocity;
        public float ultimateEnergy;
        public int carriedPoints;
        public int currentHP;
        public byte locomotionState;
        public byte abilityState;
        public byte scoringState;
        public uint tick;
        
        public GameSnapshot()
        {
            // Default constructor
        }
        
        public GameSnapshot(Snapshot snapshot)
        {
            position = snapshot.position;
            velocity = snapshot.velocity;
            ultimateEnergy = snapshot.ultimateEnergy;
            carriedPoints = snapshot.carriedPoints;
            currentHP = snapshot.currentHP;
            locomotionState = snapshot.locomotionState;
            abilityState = snapshot.abilityState;
            scoringState = snapshot.scoringState;
            tick = snapshot.tick;
        }
        
        public GameSnapshot(GameSnapshot other)
        {
            position = other.position;
            velocity = other.velocity;
            ultimateEnergy = other.ultimateEnergy;
            carriedPoints = other.carriedPoints;
            currentHP = other.currentHP;
            locomotionState = other.locomotionState;
            abilityState = other.abilityState;
            scoringState = other.scoringState;
            tick = other.tick;
        }
    }
}
