using NUnit.Framework;
using UnityEngine;
using MOBA.Input;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests that UnityInputSource returns default values when no input
    /// actions have been fired.
    /// </summary>
    public class UnityInputSourceTests
    {
        [Test]
        public void DefaultInputValuesAreZero()
        {
            // Arrange: create actions asset and enable player map
            var actions = new InputSystem_Actions();
            var player = actions.Player;
            player.Enable();

            var src = new UnityInputSource(actions);

            // Act & Assert: initial values should be zero/false
            Assert.AreEqual(0f, src.GetHorizontal(), "Horizontal input should default to 0");
            Assert.AreEqual(0f, src.GetVertical(), "Vertical input should default to 0");
            Assert.IsFalse(src.IsJumpPressed(), "Jump should not be pressed by default");

            // Cleanup
            player.Disable();
            actions.Dispose();
        }
    }
}
