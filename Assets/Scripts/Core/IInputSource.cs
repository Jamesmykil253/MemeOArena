namespace MOBA.Core
{
    /// <summary>
    /// Interface for injecting input values into gameplay logic.
    /// Abstracts away UnityEngine.Input to enable deterministic simulation and testing.
    /// Extended to handle all demo and testing input consistently.
    /// </summary>
    public interface IInputSource
    {
        float GetAxis(string axisName);
        bool GetButtonDown(string buttonName);
        bool GetButton(string buttonName);
        bool GetButtonUp(string buttonName);
        
        // Movement input
        float GetHorizontal();
        float GetVertical();
        UnityEngine.Vector2 GetMoveVector();
        
        // Action input
        bool IsJumpPressed();
        bool IsAbility1Pressed();
        bool IsAbility2Pressed();
        bool IsUltimatePressed();
        bool IsScoringPressed();
        
        // Test input for demo/debugging
        bool IsTestAddPointsPressed();
        bool IsTestDamagePressed();
        
        // Camera input
        bool IsCameraTogglePressed();
        bool IsFreePanPressed();
        
        // Extended demo input (fixing mixed input inconsistencies)
        bool IsToggleChannelPressed(); // E key for scoring channel
        bool IsSimulateDeathPressed(); // K key for death simulation
        bool IsHealPressed(); // H key for healing
        bool IsResetPressed(); // R key for reset (when not ultimate)
        
        // Keyboard input for camera panning (fixing Arrow Key access)
        bool IsArrowUpPressed();
        bool IsArrowDownPressed();
        bool IsArrowLeftPressed();
        bool IsArrowRightPressed();
        UnityEngine.Vector2 GetArrowKeyVector();
        
        // Mouse/aim input
        UnityEngine.Vector2 GetMousePosition();
        UnityEngine.Vector2 GetMouseDelta();
        bool IsMouseButtonDown(int button);
        bool IsMouseButtonUp(int button);
        bool IsRightMouseDragActive();
        
        // Scroll input for camera zoom
        float GetScrollWheelDelta();
        
        // Input state
        bool HasInputThisFrame();
        float GetInputMagnitude();
        
        // Key state queries (for consistent input handling)
        bool IsKeyPressed(UnityEngine.KeyCode key);
        bool IsKeyHeld(UnityEngine.KeyCode key);
        bool IsKeyReleased(UnityEngine.KeyCode key);
    }
}