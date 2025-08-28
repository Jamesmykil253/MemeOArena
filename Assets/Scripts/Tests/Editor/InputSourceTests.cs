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
            public bool GetButtonUp(string buttonName) => false;
            
            public float GetHorizontal() => Horizontal;
            public float GetVertical() => Vertical;
            public UnityEngine.Vector2 GetMoveVector() => new UnityEngine.Vector2(Horizontal, Vertical);
            
            public bool IsJumpPressed() => JumpDown;
            public bool IsAbility1Pressed() => false;
            public bool IsAbility2Pressed() => false;
            public bool IsUltimatePressed() => false;
            public bool IsScoringPressed() => false;
            public bool IsTestAddPointsPressed() => false;
            public bool IsTestDamagePressed() => false;
            public bool IsCameraTogglePressed() => false;
            public bool IsFreePanPressed() => false;
            
            public UnityEngine.Vector2 GetMousePosition() => UnityEngine.Vector2.zero;
            public UnityEngine.Vector2 GetMouseDelta() => UnityEngine.Vector2.zero;
            public bool IsMouseButtonDown(int button) => false;
            public bool IsMouseButtonUp(int button) => false;
            
            public bool HasInputThisFrame() => Horizontal != 0f || Vertical != 0f || JumpDown;
            public float GetInputMagnitude() => new UnityEngine.Vector2(Horizontal, Vertical).magnitude;
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