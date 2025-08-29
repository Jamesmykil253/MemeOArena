using UnityEngine;
using UnityEngine.InputSystem;
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
        private Vector2 lastMousePosition;

        public UnityInputSource(InputSystem_Actions inputActions)
        {
            this.inputActions = inputActions;
            this.inputActions.Enable();
            lastMousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }

        public float GetAxis(string axisName)
        {
            // Legacy method - map common axis names to new Input System
            switch (axisName.ToLower())
            {
                case "horizontal":
                    return GetHorizontal();
                case "vertical":
                    return GetVertical();
                default:
                    return 0f;
            }
        }

        public bool GetButtonDown(string buttonName)
        {
            // Legacy method - map common button names to new Input System
            switch (buttonName.ToLower())
            {
                case "jump":
                    return IsJumpPressed();
                case "fire1":
                    return IsMouseButtonDown(0);
                case "fire2":
                    return IsMouseButtonDown(1);
                default:
                    return false;
            }
        }

        public bool GetButton(string buttonName)
        {
            // Legacy method - map common button names to new Input System
            switch (buttonName.ToLower())
            {
                case "jump":
                    return inputActions.Player.Jump.IsPressed();
                case "fire1":
                    return Mouse.current?.leftButton.isPressed ?? false;
                case "fire2":
                    return Mouse.current?.rightButton.isPressed ?? false;
                default:
                    return false;
            }
        }
        
        public bool GetButtonUp(string buttonName)
        {
            // Legacy method - map common button names to new Input System
            switch (buttonName.ToLower())
            {
                case "jump":
                    return inputActions.Player.Jump.WasReleasedThisFrame();
                case "fire1":
                    return IsMouseButtonUp(0);
                case "fire2":
                    return IsMouseButtonUp(1);
                default:
                    return false;
            }
        }

        public float GetHorizontal()
        {
            var moveValue = inputActions.Player.Move.ReadValue<Vector2>();
            return moveValue.x;
        }

        public float GetVertical()
        {
            var moveValue = inputActions.Player.Move.ReadValue<Vector2>();
            return moveValue.y;
        }
        
        public Vector2 GetMoveVector()
        {
            Vector2 moveValue = inputActions.Player.Move.ReadValue<Vector2>();
            if (moveValue.magnitude > 0.01f)
            {
                Debug.Log($"[INPUT] Move detected: {moveValue}");
            }
            return moveValue;
        }

        public bool IsJumpPressed()
        {
            return inputActions.Player.Jump.WasPressedThisFrame();
        }
        
        public bool IsAbility1Pressed()
        {
            return inputActions.Player.Ability1.WasPressedThisFrame();
        }
        
        public bool IsAbility2Pressed()
        {
            return inputActions.Player.Ability2.WasPressedThisFrame();
        }
        
        public bool IsUltimatePressed()
        {
            return inputActions.Player.Ultimate.WasPressedThisFrame();
        }
        
        public bool IsScoringPressed()
        {
            return inputActions.Player.Scoring.WasPressedThisFrame();
        }
        
        public bool IsTestAddPointsPressed()
        {
            return inputActions.Player.TestAddPoints.WasPressedThisFrame();
        }
        
        public bool IsTestDamagePressed()
        {
            return inputActions.Player.TestDamage.WasPressedThisFrame();
        }
        
        public bool IsCameraTogglePressed()
        {
            return inputActions.Player.CameraToggle.WasPressedThisFrame();
        }
        
        public bool IsFreePanPressed()
        {
            return inputActions.Player.FreePan.WasPressedThisFrame();
        }
        
        public Vector2 GetMousePosition()
        {
            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }
        
        public Vector2 GetMouseDelta()
        {
            Vector2 currentMousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            Vector2 delta = currentMousePos - lastMousePosition;
            lastMousePosition = currentMousePos;
            return delta;
        }
        
        public bool IsMouseButtonDown(int button)
        {
            return button switch
            {
                0 => Mouse.current?.leftButton.wasPressedThisFrame ?? false,
                1 => Mouse.current?.rightButton.wasPressedThisFrame ?? false,
                2 => Mouse.current?.middleButton.wasPressedThisFrame ?? false,
                _ => false
            };
        }
        
        public bool IsMouseButtonUp(int button)
        {
            return button switch
            {
                0 => Mouse.current?.leftButton.wasReleasedThisFrame ?? false,
                1 => Mouse.current?.rightButton.wasReleasedThisFrame ?? false,
                2 => Mouse.current?.middleButton.wasReleasedThisFrame ?? false,
                _ => false
            };
        }
        
        public bool HasInputThisFrame()
        {
            Vector2 move = GetMoveVector();
            return move.magnitude > 0.01f || 
                   IsJumpPressed() || 
                   IsAbility1Pressed() || 
                   IsAbility2Pressed() || 
                   IsUltimatePressed() || 
                   IsScoringPressed() ||
                   IsMouseButtonDown(0) ||
                   IsMouseButtonDown(1);
        }
        
        public float GetInputMagnitude()
        {
            return GetMoveVector().magnitude;
        }
        
        // Extended demo input methods (fixing mixed input inconsistencies)
        public bool IsToggleChannelPressed()
        {
            return Keyboard.current?.eKey.wasPressedThisFrame ?? false;
        }
        
        public bool IsSimulateDeathPressed()
        {
            return Keyboard.current?.kKey.wasPressedThisFrame ?? false;
        }
        
        public bool IsHealPressed()
        {
            return Keyboard.current?.hKey.wasPressedThisFrame ?? false;
        }
        
        public bool IsResetPressed()
        {
            return Keyboard.current?.rKey.wasPressedThisFrame ?? false;
        }
        
        // Arrow key input for camera panning
        public bool IsArrowUpPressed()
        {
            return Keyboard.current?.upArrowKey.isPressed ?? false;
        }
        
        public bool IsArrowDownPressed()
        {
            return Keyboard.current?.downArrowKey.isPressed ?? false;
        }
        
        public bool IsArrowLeftPressed()
        {
            return Keyboard.current?.leftArrowKey.isPressed ?? false;
        }
        
        public bool IsArrowRightPressed()
        {
            return Keyboard.current?.rightArrowKey.isPressed ?? false;
        }
        
        public Vector2 GetArrowKeyVector()
        {
            Vector2 arrowInput = Vector2.zero;
            if (IsArrowUpPressed()) arrowInput.y += 1f;
            if (IsArrowDownPressed()) arrowInput.y -= 1f;
            if (IsArrowLeftPressed()) arrowInput.x -= 1f;
            if (IsArrowRightPressed()) arrowInput.x += 1f;
            return arrowInput;
        }
        
        // Right mouse drag detection
        public bool IsRightMouseDragActive()
        {
            return Mouse.current?.rightButton.isPressed ?? false;
        }
        
        // Scroll wheel input
        public float GetScrollWheelDelta()
        {
            return Mouse.current?.scroll.ReadValue().y ?? 0f;
        }
        
        // General key state queries
        public bool IsKeyPressed(KeyCode key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            
            var keyControl = GetKeyControlFromKeyCode(keyboard, key);
            return keyControl?.wasPressedThisFrame ?? false;
        }
        
        public bool IsKeyHeld(KeyCode key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            
            var keyControl = GetKeyControlFromKeyCode(keyboard, key);
            return keyControl?.isPressed ?? false;
        }
        
        public bool IsKeyReleased(KeyCode key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            
            var keyControl = GetKeyControlFromKeyCode(keyboard, key);
            return keyControl?.wasReleasedThisFrame ?? false;
        }
        
        private UnityEngine.InputSystem.Controls.KeyControl GetKeyControlFromKeyCode(Keyboard keyboard, KeyCode keyCode)
        {
            // Map common KeyCode values to Input System KeyControl
            // This is a simplified mapping for demo purposes
            return keyCode switch
            {
                KeyCode.Space => keyboard.spaceKey,
                KeyCode.E => keyboard.eKey,
                KeyCode.K => keyboard.kKey,
                KeyCode.H => keyboard.hKey,
                KeyCode.R => keyboard.rKey,
                KeyCode.P => keyboard.pKey,
                KeyCode.T => keyboard.tKey,
                KeyCode.C => keyboard.cKey,
                KeyCode.V => keyboard.vKey,
                KeyCode.Q => keyboard.qKey,
                KeyCode.W => keyboard.wKey,
                KeyCode.A => keyboard.aKey,
                KeyCode.S => keyboard.sKey,
                KeyCode.D => keyboard.dKey,
                KeyCode.F1 => keyboard.f1Key,
                KeyCode.Escape => keyboard.escapeKey,
                KeyCode.UpArrow => keyboard.upArrowKey,
                KeyCode.DownArrow => keyboard.downArrowKey,
                KeyCode.LeftArrow => keyboard.leftArrowKey,
                KeyCode.RightArrow => keyboard.rightArrowKey,
                _ => null
            };
        }
    }
}