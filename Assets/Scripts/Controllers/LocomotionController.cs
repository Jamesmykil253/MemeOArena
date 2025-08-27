using UnityEngine;

/// <summary>
/// Simplified locomotion controller that consumes an injected input source for horizontal movement and jump.
/// Exposes a DesiredVelocity property used by physics integration elsewhere.
/// </summary>
public class LocomotionController : MonoBehaviour
{
    public IInputSource InputSource { get; private set; }
    public Vector3 DesiredVelocity { get; private set; }

    /// <summary>
    /// Initialize the controller with an input source. Call this before updating.
    /// </summary>
    public void Initialize(IInputSource inputSource)
    {
        InputSource = inputSource;
    }

    void Update()
    {
        // Fall back to zero input when no source provided.
        float h = InputSource?.GetAxis("Horizontal") ?? 0f;
        bool jumpDown = InputSource?.GetButtonDown("Jump") ?? false;

        // Horizontal desired velocity is directly the horizontal axis value. In a real implementation,
        // this would be scaled by move speed and subject to ground/air movement rules.
        DesiredVelocity = new Vector3(h, 0f, 0f);

        if (jumpDown)
        {
            // In a real implementation, this would trigger state changes in the locomotion FSM.
        }
    }
}