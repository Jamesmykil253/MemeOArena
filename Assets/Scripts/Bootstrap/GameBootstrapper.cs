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
    /// Assign the relevant ScriptableObjects in the inspector.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Data Definitions")]
        public BaseStatsTemplate baseStats;
        public UltimateEnergyDef ultimateDef;
        public ScoringDef scoringDef;
        public AbilityDef[] abilities;
        public MOBA.Input.InputSystem_Actions inputActions;

        private PlayerContext context;
        private LocomotionController locomotion;
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
            locomotion = new LocomotionController(context, inputSource);
            abilityCtrl = new AbilityController(context);
            scoringCtrl = new ScoringController(context);
            spawnMachine = new SpawnMachine(context);
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
            locomotion.Update(dt);
            abilityCtrl.Update(dt);
            scoringCtrl.Update(dt);
            spawnMachine.Update(dt);
        }
    }
}
