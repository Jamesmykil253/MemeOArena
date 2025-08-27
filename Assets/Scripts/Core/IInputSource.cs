using UnityEngine;

namespace MOBA.Core
{
    /// <summary>
    /// Abstraction for player input.  Implementations provide
    /// directional movement and jump commands for a given tick.
    /// Decouples input polling from gameplay logic to support
    /// deterministic simulation and server authority.
    /// </summary>
    public interface IInputSource
    {
        /// <summary>
        /// Get the horizontal axis input for this tick.
        /// Values should be in the range [-1, 1].
        /// </summary>
        float GetHorizontal();

        /// <summary>
        /// Get the vertical axis input for this tick.
        /// Values should be in the range [-1, 1].
        /// </summary>
        float GetVertical();

        /// <summary>
        /// Return true if the jump command was pressed on this tick.
        /// This should return true only once per press.
        /// </summary>
        bool IsJumpPressed();
    }
}
