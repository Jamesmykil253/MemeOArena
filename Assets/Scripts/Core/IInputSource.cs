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
        
        // Additional convenience methods
        float GetHorizontal();
        float GetVertical();
        bool IsJumpPressed();
    }
}