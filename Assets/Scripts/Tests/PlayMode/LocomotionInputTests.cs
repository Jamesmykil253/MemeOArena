using NUnit.Framework;
using UnityEngine;
using MOBA.Data;
using MOBA.Core;
using MOBA.Controllers;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests that LocomotionController uses an injected IInputSource
    /// and produces expected DesiredVelocity in the grounded state.
    /// </summary>
    public class LocomotionInputTests
    {
        private class MockInputSource : IInputSource
        {
            public float horizontal;
            public float vertical;
            public bool jump;

            public float GetAxis(string axisName)
            {
                if (axisName == "Horizontal") return horizontal;
                if (axisName == "Vertical") return vertical;
                return 0f;
            }
            
            public bool GetButtonDown(string buttonName) => buttonName == "Jump" && jump;
            public bool GetButton(string buttonName) => false;

            public float GetHorizontal() => horizontal;
            public float GetVertical() => vertical;
            public bool IsJumpPressed()
            {
                bool result = jump;
                // simulate one-shot behaviour
                jump = false;
                return result;
            }
        }

        [Test]
        public void HorizontalMovementProducesDesiredVelocity()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MoveSpeed = 5f;
            var ctx = new PlayerContext("player1", baseStats, null, null);
            var input = new MockInputSource { horizontal = 1f, vertical = 0f };
            var controller = new LocomotionController(ctx, input);

            controller.Update(0.1f);

            Assert.AreEqual(new Vector3(ctx.moveSpeed, 0f, 0f), controller.DesiredVelocity);
        }

        [Test]
        public void DiagonalMovementIsNormalized()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MoveSpeed = 5f;
            var ctx = new PlayerContext("player2", baseStats, null, null);
            var input = new MockInputSource { horizontal = 1f, vertical = 1f };
            var controller = new LocomotionController(ctx, input);
            controller.Update(0.1f);
            float mag = controller.DesiredVelocity.magnitude;
            Assert.That(Mathf.Approximately(mag, ctx.moveSpeed), "Diagonal movement should be normalized to moveSpeed");
        }

        [Test]
        public void NoMovementProducesZeroVelocity()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MoveSpeed = 5f;
            var ctx = new PlayerContext("player3", baseStats, null, null);
            var input = new MockInputSource { horizontal = 0f, vertical = 0f };
            var controller = new LocomotionController(ctx, input);
            controller.Update(0.1f);
            Assert.AreEqual(Vector3.zero, controller.DesiredVelocity);
        }
    }
}
