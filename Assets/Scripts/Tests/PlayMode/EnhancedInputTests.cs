using NUnit.Framework;
using UnityEngine;
using MOBA.Input;
using MOBA.Core;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for enhanced input system functionality
    /// </summary>
    public class EnhancedInputTests
    {
        [Test]
        public void UnityInputSourceProvidesAllInputMethods()
        {
            var inputActions = new MOBA.Input.InputSystem_Actions();
            var inputSource = new UnityInputSource(inputActions);
            
            // Test that all interface methods return valid values (not testing method group references)
            Assert.AreEqual(0f, inputSource.GetHorizontal());
            Assert.AreEqual(0f, inputSource.GetVertical());
            Assert.AreEqual(Vector2.zero, inputSource.GetMoveVector());
            Assert.IsFalse(inputSource.IsJumpPressed());
            Assert.IsFalse(inputSource.IsAbility1Pressed());
            Assert.IsFalse(inputSource.IsAbility2Pressed());
            Assert.IsFalse(inputSource.IsUltimatePressed());
            Assert.IsFalse(inputSource.IsScoringPressed());
            Assert.AreEqual(Vector2.zero, inputSource.GetMousePosition());
            Assert.AreEqual(Vector2.zero, inputSource.GetMouseDelta());
            Assert.IsFalse(inputSource.HasInputThisFrame());
            Assert.AreEqual(0f, inputSource.GetInputMagnitude());
        }
        
        [Test]
        public void MockInputSourceWorksCorrectly()
        {
            var mockInput = new MockInputSource();
            
            // Test default values
            Assert.AreEqual(0f, mockInput.GetHorizontal());
            Assert.AreEqual(0f, mockInput.GetVertical());
            Assert.IsFalse(mockInput.IsJumpPressed());
            Assert.IsFalse(mockInput.HasInputThisFrame());
            Assert.AreEqual(0f, mockInput.GetInputMagnitude());
        }
        
        [Test]
        public void InputBufferTimingWorks()
        {
            var bufferedInput = new BufferedInput
            {
                buttonName = "Jump",
                pressed = true,
                timestamp = Time.time,
                tick = 100u
            };
            
            Assert.AreEqual("Jump", bufferedInput.buttonName);
            Assert.IsTrue(bufferedInput.pressed);
            Assert.AreEqual(100u, bufferedInput.tick);
        }
        
        private class MockInputSource : IInputSource
        {
            public float horizontal = 0f;
            public float vertical = 0f;
            public bool jump = false;
            
            public float GetAxis(string axisName) => 0f;
            public bool GetButtonDown(string buttonName) => false;
            public bool GetButton(string buttonName) => false;
            public bool GetButtonUp(string buttonName) => false;
            public float GetHorizontal() => horizontal;
            public float GetVertical() => vertical;
            public Vector2 GetMoveVector() => new Vector2(horizontal, vertical);
            public bool IsJumpPressed() => jump;
            public bool IsAbility1Pressed() => false;
            public bool IsAbility2Pressed() => false;
            public bool IsUltimatePressed() => false;
            public bool IsScoringPressed() => false;
            public bool IsTestAddPointsPressed() => false;
            public bool IsTestDamagePressed() => false;
            public bool IsCameraTogglePressed() => false;
            public bool IsFreePanPressed() => false;
            public Vector2 GetMousePosition() => Vector2.zero;
            public Vector2 GetMouseDelta() => Vector2.zero;
            public bool IsMouseButtonDown(int button) => false;
            public bool IsMouseButtonUp(int button) => false;
            public bool HasInputThisFrame() => horizontal != 0f || vertical != 0f || jump;
            public float GetInputMagnitude() => new Vector2(horizontal, vertical).magnitude;
            
            // Extended input methods
            public bool IsToggleChannelPressed() => false;
            public bool IsSimulateDeathPressed() => false;
            public bool IsHealPressed() => false;
            public bool IsResetPressed() => false;
            public bool IsArrowUpPressed() => false;
            public bool IsArrowDownPressed() => false;
            public bool IsArrowLeftPressed() => false;
            public bool IsArrowRightPressed() => false;
            public Vector2 GetArrowKeyVector() => Vector2.zero;
            public bool IsRightMouseDragActive() => false;
            public float GetScrollWheelDelta() => 0f;
            public bool IsKeyPressed(UnityEngine.KeyCode key) => false;
            public bool IsKeyHeld(UnityEngine.KeyCode key) => false;
            public bool IsKeyReleased(UnityEngine.KeyCode key) => false;
        }
    }
}
