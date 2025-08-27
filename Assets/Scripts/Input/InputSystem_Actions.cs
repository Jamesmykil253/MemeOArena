using UnityEngine;

namespace MOBA.Input
{
    /// <summary>
    /// Stub implementation of Unity's auto-generated InputSystem_Actions class.
    /// This allows the project to compile without the actual input actions asset.
    /// </summary>
    public class InputSystem_Actions
    {
        public PlayerActions Player => new PlayerActions();

        public class PlayerActions
        {
            public MoveAction Move => new MoveAction();
            public JumpAction Jump => new JumpAction();

            public void Enable() { }
            public void Disable() { }

            public class MoveAction
            {
                public T ReadValue<T>() => default(T);
            }

            public class JumpAction
            {
                public bool WasPressedThisFrame() => false;
                public T ReadValue<T>() => default(T);
            }
        }

        public void Enable() { }
        public void Disable() { }
        public void Dispose() { }
    }
}
