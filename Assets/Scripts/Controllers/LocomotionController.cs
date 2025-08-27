using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Controllers
{
    /// <summary>
    /// Simplified locomotion controller that consumes an injected input source for horizontal movement and jump.
    /// Exposes a DesiredVelocity property used by physics integration elsewhere.
    /// </summary>
    public class LocomotionController
    {
        private readonly PlayerContext context;
        public IInputSource InputSource { get; private set; }
        public Vector3 DesiredVelocity { get; private set; }

        /// <summary>
        /// Initialize the controller with player context and input source.
        /// </summary>
        public LocomotionController(PlayerContext ctx, IInputSource inputSource)
        {
            context = ctx;
            InputSource = inputSource;
        }

        public void Update(float dt)
        {
            // Fall back to zero input when no source provided.
            float h = InputSource?.GetHorizontal() ?? 0f;
            float v = InputSource?.GetVertical() ?? 0f;
            bool jumpPressed = InputSource?.IsJumpPressed() ?? false;

            // Scale by move speed from player context
            float moveSpeed = context.moveSpeed;
            
            // Horizontal desired velocity is the input scaled by move speed. In a real implementation,
            // this would be subject to ground/air movement rules and FSM states.
            DesiredVelocity = new Vector3(h * moveSpeed, 0f, v * moveSpeed);

            if (jumpPressed)
            {
                // In a real implementation, this would trigger state changes in the locomotion FSM.
            }
        }
    }
}