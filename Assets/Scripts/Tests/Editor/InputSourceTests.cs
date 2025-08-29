using NUnit.Framework;
using UnityEngine;
using MOBA.Core;
using MOBA.Controllers;
using MOBA.Data;

namespace Tests.Editor
{
    /// <summary>
    /// Simple test implementation of ILocomotionController for unit testing.
    /// </summary>
    public class TestLocomotionController : ILocomotionController
    {
        private PlayerContext context;
        private IInputSource inputSource;
        private Vector3 desiredVelocity;
        private bool isGrounded = true;
        private float moveSpeed = 5f;
        private bool isEnabled = true;
        
        public Vector3 DesiredVelocity => desiredVelocity;
        public bool IsGrounded => isGrounded;
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        
        public event System.Action OnJump;
        public event System.Action OnLand;
        public event System.Action<Vector3> OnKnockbackStart;
        public event System.Action OnKnockbackEnd;

        public TestLocomotionController(PlayerContext playerContext, IInputSource input)
        {
            context = playerContext;
            inputSource = input;
            if (playerContext != null)
                moveSpeed = playerContext.moveSpeed;
        }

        public void Initialize(PlayerContext playerContext, IInputSource input)
        {
            context = playerContext;
            inputSource = input;
            if (playerContext != null)
                moveSpeed = playerContext.moveSpeed;
        }

        public void Tick(float deltaTime) => Update(deltaTime);

        public void Update(float dt)
        {
            if (!isEnabled) 
            {
                desiredVelocity = Vector3.zero;
                return;
            }

            float h = inputSource?.GetHorizontal() ?? 0f;
            float v = inputSource?.GetVertical() ?? 0f;
            bool jumpPressed = inputSource?.IsJumpPressed() ?? false;

            desiredVelocity = new Vector3(h * moveSpeed, 0f, v * moveSpeed);
            if (jumpPressed) TryJump();
        }

        public void TryJump()
        {
            if (isGrounded)
            {
                OnJump?.Invoke();
                isGrounded = false;
                isGrounded = true;
                OnLand?.Invoke();
            }
        }

        public void ApplyKnockback(Vector3 direction, float force, float duration)
        {
            OnKnockbackStart?.Invoke(direction);
            desiredVelocity = direction.normalized * force;
            OnKnockbackEnd?.Invoke();
        }
    }

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
            
            // Extended input methods
            public bool IsToggleChannelPressed() => false;
            public bool IsSimulateDeathPressed() => false;
            public bool IsHealPressed() => false;
            public bool IsResetPressed() => false;
            public bool IsArrowUpPressed() => false;
            public bool IsArrowDownPressed() => false;
            public bool IsArrowLeftPressed() => false;
            public bool IsArrowRightPressed() => false;
            public UnityEngine.Vector2 GetArrowKeyVector() => UnityEngine.Vector2.zero;
            public bool IsRightMouseDragActive() => false;
            public float GetScrollWheelDelta() => 0f;
            public bool IsKeyPressed(UnityEngine.KeyCode key) => false;
            public bool IsKeyHeld(UnityEngine.KeyCode key) => false;
            public bool IsKeyReleased(UnityEngine.KeyCode key) => false;
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
            var controller = new TestLocomotionController(context, input);
            
            controller.Update(0.016f);
            Assert.AreEqual(5f, controller.DesiredVelocity.x, 1e-4f);
        }
    }
}