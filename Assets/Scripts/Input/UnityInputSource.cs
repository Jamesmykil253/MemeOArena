using UnityEngine;
using MOBA.Core;

namespace MOBA.Input
{
    /// <summary>
    /// Concrete implementation of IInputSource that delegates to Unity's new Input System via InputSystem_Actions.
    /// This allows gameplay code to remain agnostic to the actual input API, enabling deterministic testing
    /// and simulation when paired with a mock or recorded input source.
    /// </summary>
    public class UnityInputSource : IInputSource
    {
        private readonly InputSystem_Actions inputActions;

        public UnityInputSource(InputSystem_Actions inputActions)
        {
            this.inputActions = inputActions;
            this.inputActions.Enable();
        }

        public float GetAxis(string axisName)
        {
            return UnityEngine.Input.GetAxisRaw(axisName);
        }

        public bool GetButtonDown(string buttonName)
        {
            return UnityEngine.Input.GetButtonDown(buttonName);
        }

        public bool GetButton(string buttonName)
        {
            return UnityEngine.Input.GetButton(buttonName);
        }

        public float GetHorizontal()
        {
            var moveValue = inputActions.Player.Move.ReadValue<UnityEngine.Vector2>();
            return moveValue.x;
        }

        public float GetVertical()
        {
            var moveValue = inputActions.Player.Move.ReadValue<UnityEngine.Vector2>();
            return moveValue.y;
        }

        public bool IsJumpPressed()
        {
            return inputActions.Player.Jump.WasPressedThisFrame();
        }
    }
}