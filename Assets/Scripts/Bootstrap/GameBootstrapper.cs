using UnityEngine;
using MOBA.Data;
using MOBA.Controllers;
using MOBA.Spawn;
using MOBA.Input;
using MOBA.Core;

namespace MOBA.Bootstrap
{
    /// <summary>
    /// Wires together the core game systems for a single player in a scene.
    /// Enhanced with networking and physics integration capabilities.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Data Definitions")]
        public BaseStatsTemplate baseStats;
        public UltimateEnergyDef ultimateDef;
        public ScoringDef scoringDef;
        public AbilityDef[] abilities;
        public MOBA.Input.InputSystem_Actions inputActions;
        
        [Header("Physics Settings")]
        public JumpPhysicsDef jumpPhysics;
        
        [Header("System Settings")]
        public bool enableEnhancedInput = true;

        private PlayerContext context;
        private ILocomotionController locomotion;
        private AbilityController abilityCtrl;
        private ScoringController scoringCtrl;
        private SpawnMachine spawnMachine;
        private UnityInputSource inputSource;

        /// <summary>
        /// Perform initialization. Separated from Awake for testability.
        /// </summary>
        public void Initialize()
        {
            // Create context
            context = new PlayerContext("player", baseStats, ultimateDef, scoringDef);
            context.abilities.AddRange(abilities);
            
            // Create input source
            inputSource = new UnityInputSource(inputActions);
            
            // Initialize controllers
            var locomotionGameObject = new GameObject("LocomotionController");
            var unifiedLocomotion = locomotionGameObject.AddComponent<UnifiedLocomotionController>();
            unifiedLocomotion.Initialize(context, inputSource);
            locomotion = unifiedLocomotion;
            abilityCtrl = new AbilityController(context);
            scoringCtrl = new ScoringController(context);
            spawnMachine = new SpawnMachine(context);
            
            // Log system initialization
            Debug.Log("GameBootstrapper: Enhanced systems initialized");
        }

        private void Awake()
        {
            Initialize();
            // For demonstration, begin spawn immediately
            spawnMachine.SpawnRequest();
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            locomotion.Tick(dt);
            abilityCtrl.Update(dt);
            scoringCtrl.Update(dt);
            spawnMachine.Update(dt);
        }
        
        /// <summary>
        /// Get the current player context
        /// </summary>
        public PlayerContext GetPlayerContext()
        {
            return context;
        }
        
        /// <summary>
        /// Apply damage to this player
        /// </summary>
        public void TakeDamage(int amount)
        {
            context.ApplyDamage(amount);
            Debug.Log($"Player took {amount} damage, HP: {context.currentHP}");
        }
        
        /// <summary>
        /// Add points to carry
        /// </summary>
        public void AddPoints(int amount)
        {
            scoringCtrl.AddPoints(amount);
            Debug.Log($"Player picked up {amount} points, carrying: {context.carriedPoints}");
        }
        
        /// <summary>
        /// Access to locomotion controller (public for testing)
        /// </summary>
        public ILocomotionController GetLocomotionController()
        {
            return locomotion;
        }
        
        /// <summary>
        /// Access to scoring controller (public for testing)
        /// </summary>
        public ScoringController GetScoringController()
        {
            return scoringCtrl;
        }
    }
}
