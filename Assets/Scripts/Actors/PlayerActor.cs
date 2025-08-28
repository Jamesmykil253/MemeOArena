using UnityEngine;
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
        private LocomotionController locomotion;
        private AbilityController abilityCtrl;
        private CharacterController charController;

        private void Awake()
        {
            bootstrap = GetComponent<GameBootstrapper>();
            // Access controllers from the bootstrapper using reflection
            // because they are private. Alternatively, expose properties.
            var locomotionField = typeof(GameBootstrapper).GetField("locomotion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            locomotion = (LocomotionController)locomotionField.GetValue(bootstrap);
            var abilityField = typeof(GameBootstrapper).GetField("abilityCtrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            abilityCtrl = (AbilityController)abilityField.GetValue(bootstrap);

            charController = GetComponent<CharacterController>();
        }

        private void FixedUpdate()
        {
            // Move the player using desired velocity
            Vector3 velocity = locomotion.DesiredVelocity;
            float dt = Time.fixedDeltaTime;
            if (charController != null)
            {
                // CharacterController.Move applies gravity; omit y component from desired velocity
                charController.Move(new Vector3(velocity.x, 0f, velocity.z) * dt);
            }
            else
            {
                transform.position += velocity * dt;
            }
        }

        private void Update()
        {
            // Legacy demo input handlers - now handled by proper input system
            // in DemoPlayerController and ability systems
            /*
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Ability 1 pressed");
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Ability 2 pressed");
            }
            */
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
