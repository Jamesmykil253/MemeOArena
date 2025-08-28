namespace MOBA.Core
{
    /// <summary>
    /// Interface for injecting input values into gameplay logic.
    /// Abstracts away UnityEngine.Input to enable deterministic simulation and testing.
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
        
        // Mouse/aim input
        UnityEngine.Vector2 GetMousePosition();
        UnityEngine.Vector2 GetMouseDelta();
        bool IsMouseButtonDown(int button);
        bool IsMouseButtonUp(int button);
        
        // Input state
        bool HasInputThisFrame();
        float GetInputMagnitude();
    }
}