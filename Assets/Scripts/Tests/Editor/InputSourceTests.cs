using NUnit.Framework;
using UnityEngine;
using MOBA.Core;
using MOBA.Controllers;
using MOBA.Data;

namespace Tests.Editor
{
    /// <summary>
    /// Tests that the locomotion controller reads input from an injected input source rather than UnityEngine.Input.
    /// </summary>
    public class InputSourceTests
    {
        private class TestInputSource : IInputSource
        {
            public float Horizontal;
            public float Vertical;
            public bool JumpDown;
            public float GetAxis(string axisName) => axisName == "Horizontal" ? Horizontal : Vertical;
            public bool GetButtonDown(string buttonName) => JumpDown;
            public bool GetButton(string buttonName) => false;
            public float GetHorizontal() => Horizontal;
            public float GetVertical() => Vertical;
            public bool IsJumpPressed() => JumpDown;
        }

        [Test]
        public void LocomotionReadsInjectedInput()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MoveSpeed = 5f;
            
            var ultimateDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            var context = new PlayerContext("test", baseStats, ultimateDef, scoringDef);
            
            var input = new TestInputSource { Horizontal = 1f, JumpDown = true };
            var controller = new LocomotionController(context, input);
            
            controller.Update(0.016f);
            Assert.AreEqual(5f, controller.DesiredVelocity.x, 1e-4f);
        }
    }
}