using UnityEngine;

/// <summary>
/// Concrete implementation of IInputSource that delegates to UnityEngine.Input. This allows
/// gameplay code to remain agnostic to the actual input API, enabling deterministic testing
/// and simulation when paired with a mock or recorded input source.
/// </summary>
public class UnityInputSource : IInputSource
{
    public float GetAxis(string axisName)
    {
        return Input.GetAxisRaw(axisName);
    }

    public bool GetButtonDown(string buttonName)
    {
        return Input.GetButtonDown(buttonName);
    }

    public bool GetButton(string buttonName)
    {
        return Input.GetButton(buttonName);
    }
}