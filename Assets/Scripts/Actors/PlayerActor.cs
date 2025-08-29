using UnityEngine;
using UnityEngine.InputSystem;
using MOBA.Bootstrap;
using MOBA.Controllers;

namespace MOBA.Actors
{
    /// <summary>
    /// Drives a player's movement and ability casting by reading from
    /// the LocomotionController and AbilityController. Attach this to the
    /// same GameObject as the GameBootstrapper and a CharacterController
    /// (or use transform movement if no CharacterController is present).
    /// </summary>
    [RequireComponent(typeof(GameBootstrapper))]
    public class PlayerActor : MonoBehaviour
    {
        private GameBootstrapper bootstrap;
        private ILocomotionController locomotion;
        private AbilityController abilityCtrl;
        private CharacterController charController;

        private void Awake()
        {
            bootstrap = GetComponent<GameBootstrapper>();
            // Access controllers from the bootstrapper using reflection
            // because they are private. Alternatively, expose properties.
            var locomotionField = typeof(GameBootstrapper).GetField("locomotion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            locomotion = (ILocomotionController)locomotionField.GetValue(bootstrap);
            var abilityField = typeof(GameBootstrapper).GetField("abilityCtrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            abilityCtrl = (AbilityController)abilityField.GetValue(bootstrap);

            charController = GetComponent<CharacterController>();
        }

        // MOVEMENT HANDLING REMOVED - UnifiedLocomotionController now handles all movement
        // This prevents double movement application that was causing jittery/erratic behavior

        private void Update()
        {
            // Input is now handled by proper input system through IInputSource
            // in DemoPlayerController and ability systems - no direct input calls here
        }

        /// <summary>
        /// Example method to simulate picking up points. Call from collision or trigger.
        /// </summary>
        /// <param name="amount">Number of points to add.</param>
        public void PickupPoints(int amount)
        {
            // Access scoring controller via reflection
            var scoringField = typeof(GameBootstrapper).GetField("scoringCtrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scoring = (ScoringController)scoringField.GetValue(bootstrap);
            scoring.AddPoints(amount);
        }
    }
}
