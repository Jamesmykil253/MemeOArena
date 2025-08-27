using UnityEngine;
using UnityEngine.InputSystem;
using MOBA.Core;

namespace MOBA.Input
{
    /// <summary>
    /// Adapts the generated InputSystem_Actions to the IInputSource interface.
    /// This class should be instantiated on the client and passed to
    /// LocomotionController to feed movement and jump commands.
    /// </summary>
    public class UnityInputSource : IInputSource
    {
        private readonly InputSystem_Actions.PlayerActions player;

        /// <summary>
        /// Construct a new UnityInputSource from an InputSystem_Actions asset.
        /// The asset must have its Player action map enabled before use.
        /// </summary>
        /// <param name="actions">The generated actions asset.</param>
        public UnityInputSource(InputSystem_Actions actions)
        {
            player = actions.Player;
        }

        /// <inheritdoc />
        public float GetHorizontal()
        {
            Vector2 move = player.Move.ReadValue<Vector2>();
            return move.x;
        }

        /// <inheritdoc />
        public float GetVertical()
        {
            Vector2 move = player.Move.ReadValue<Vector2>();
            return move.y;
        }

        /// <inheritdoc />
        public bool IsJumpPressed()
        {
            // Use WasPressedThisFrame to emulate one-shot behaviour.
            return player.Jump.WasPressedThisFrame();
        }
    }
}
