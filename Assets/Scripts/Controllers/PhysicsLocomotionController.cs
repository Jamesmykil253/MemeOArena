using System;
using UnityEngine;
using MOBA.Core;
using MOBA.Data;
using MOBA.Physics;

namespace MOBA.Controllers
{
    /// <summary>
    /// Enhanced locomotion controller with physics integration and FSM states.
    /// Handles grounded/airborne transitions, jumping, and knockback states.
    /// </summary>
    public class PhysicsLocomotionController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 15f;
        
        [Header("Jump Settings")]
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;
        [SerializeField] private bool allowDoubleJump = false;
        [SerializeField] private float doubleJumpForce = 8f;
        
        // Components
        private DeterministicPhysics physicsSystem;
        private IInputSource inputSource;
        private StateMachine fsm;
        private TickManager tickManager;
        
        // States
        private GroundedState groundedState;
        private AirborneState airborneState;
        private KnockbackState knockbackState;
        private DisabledState disabledState;
        
        // State tracking
        private PlayerContext context;
        private string physicsBodyId;
        private Vector3 desiredVelocity;
        private float lastGroundedTime;
        private float jumpBufferTimer;
        private bool hasDoubleJump;
        
        public Vector3 DesiredVelocity => desiredVelocity;
        public bool IsGrounded => physicsSystem.GetBody(physicsBodyId)?.isGrounded ?? false;
        public bool IsInKnockback => physicsSystem.GetBody(physicsBodyId)?.isInKnockback ?? false;
        public StateMachine FSM => fsm;
        
        // Events
        public event Action OnJump;
        public event Action OnLand;
        public event Action<Vector3> OnKnockbackStart;
        public event Action OnKnockbackEnd;
        
        private void Awake()
        {
            tickManager = FindFirstObjectByType<TickManager>();
            if (tickManager == null)
            {
                Debug.LogError("PhysicsLocomotionController: TickManager not found in scene");
            }
        }
        
        private void Start()
        {
            RegisterPhysicsBody();
            fsm.Change(groundedState);
        }
        
        /// <summary>
        /// Initialize the controller with context and input source
        /// </summary>
        public void Initialize(PlayerContext playerContext, IInputSource input)
        {
            context = playerContext;
            inputSource = input;
            physicsBodyId = playerContext.playerId + "_body";
            
            // Update move speed from context
            if (playerContext.baseStats != null)
            {
                moveSpeed = playerContext.baseStats.MoveSpeed;
            }
        }
        
        private void SetupStateMachine()
        {
            fsm = new StateMachine("LocomotionFSM", "locomotion");
            
            // Create states
            groundedState = new GroundedState(this);
            airborneState = new AirborneState(this);
            knockbackState = new KnockbackState(this);
            disabledState = new DisabledState(this);
        }
        
        private void RegisterPhysicsBody()
        {
            if (physicsSystem == null) return;
            
            PhysicsBodySettings settings = PhysicsBodySettings.Default;
            settings.mass = 1f;
            settings.useGravity = true;
            
            physicsSystem.RegisterBody(physicsBodyId, transform.position, Vector3.zero, settings);
        }
        
        private void Update()
        {
            float dt = Time.deltaTime;
            
            // Update timers
            UpdateTimers(dt);
            
            // Process input
            ProcessInput();
            
            // Update FSM
            fsm.Update(dt);
            
            // Apply movement
            ApplyMovement();
            
            // Sync transform with physics
            SyncTransformWithPhysics();
        }
        
        private void UpdateTimers(float dt)
        {
            // Update jump buffer timer
            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= dt;
            }
            
            // Update coyote time
            if (!IsGrounded)
            {
                lastGroundedTime += dt;
            }
            else
            {
                lastGroundedTime = 0f;
            }
        }
        
        private void ProcessInput()
        {
            if (inputSource == null) return;
            
            // Get movement input
            float horizontal = inputSource.GetHorizontal();
            float vertical = inputSource.GetVertical();
            Vector2 moveInput = new Vector2(horizontal, vertical);
            
            // Apply deadzone
            if (moveInput.magnitude < 0.1f)
            {
                moveInput = Vector2.zero;
            }
            
            // Calculate desired velocity
            Vector3 worldInput = new Vector3(moveInput.x, 0f, moveInput.y);
            desiredVelocity = worldInput.normalized * moveSpeed;
            
            // Check for jump input
            if (inputSource.IsJumpPressed())
            {
                jumpBufferTimer = jumpBufferTime;
            }
        }
        
        private void ApplyMovement()
        {
            if (physicsSystem == null) return;
            
            PhysicsBody body = physicsSystem.GetBody(physicsBodyId);
            if (body == null) return;
            
            // Don't override movement during knockback
            if (body.isInKnockback) return;
            
            // Apply horizontal movement
            Vector3 currentVel = body.velocity;
            Vector3 targetHorizontalVel = new Vector3(desiredVelocity.x, 0f, desiredVelocity.z);
            Vector3 currentHorizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);
            
            // Lerp toward target velocity
            float lerpSpeed = desiredVelocity.magnitude > 0.1f ? acceleration : deceleration;
            Vector3 newHorizontalVel = Vector3.Lerp(currentHorizontalVel, targetHorizontalVel, 
                                                   lerpSpeed * Time.deltaTime);
            
            // Maintain vertical velocity
            Vector3 newVelocity = new Vector3(newHorizontalVel.x, currentVel.y, newHorizontalVel.z);
            physicsSystem.SetBodyVelocity(physicsBodyId, newVelocity);
        }
        
        private void SyncTransformWithPhysics()
        {
            if (physicsSystem == null) return;
            
            PhysicsBody body = physicsSystem.GetBody(physicsBodyId);
            if (body != null)
            {
                transform.position = body.position;
            }
        }
        
        /// <summary>
        /// Attempt to jump
        /// </summary>
        public void TryJump()
        {
            bool canJump = false;
            
            if (IsGrounded || lastGroundedTime <= coyoteTime)
            {
                // Regular jump
                canJump = true;
                hasDoubleJump = allowDoubleJump;
            }
            else if (allowDoubleJump && hasDoubleJump)
            {
                // Double jump
                canJump = true;
                hasDoubleJump = false;
            }
            
            if (canJump)
            {
                PerformJump();
            }
        }
        
        private void PerformJump()
        {
            if (physicsSystem == null) return;
            
            float jumpVelocity = hasDoubleJump && !IsGrounded ? doubleJumpForce : jumpForce;
            Vector3 jumpImpulse = Vector3.up * jumpVelocity;
            
            physicsSystem.ApplyImpulse(physicsBodyId, jumpImpulse);
            jumpBufferTimer = 0f;
            
            OnJump?.Invoke();
            fsm.Change(airborneState, "Jump");
        }
        
        /// <summary>
        /// Apply knockback force
        /// </summary>
        public void ApplyKnockback(Vector3 direction, float force, float duration)
        {
            if (physicsSystem == null) return;
            
            physicsSystem.ApplyKnockback(physicsBodyId, direction, force, duration);
            fsm.Change(knockbackState, "Knockback applied");
            
            OnKnockbackStart?.Invoke(direction);
        }
        
        /// <summary>
        /// Disable locomotion (for abilities, etc.)
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                fsm.Change(IsGrounded ? groundedState : airborneState, "Enabled");
            }
            else
            {
                fsm.Change(disabledState, "Disabled");
            }
        }
        
        #region State Classes
        
        private class GroundedState : IState
        {
            private readonly PhysicsLocomotionController controller;
            
            public GroundedState(PhysicsLocomotionController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter()
            {
                controller.hasDoubleJump = controller.allowDoubleJump;
                controller.OnLand?.Invoke();
            }
            
            public void Exit() { }
            
            public void Tick(float dt)
            {
                // Check for jump input
                if (controller.jumpBufferTimer > 0f)
                {
                    controller.TryJump();
                }
                
                // Check if we've left the ground
                if (!controller.IsGrounded)
                {
                    controller.fsm.Change(controller.airborneState, "Left ground");
                }
                
                // Check for knockback
                if (controller.IsInKnockback)
                {
                    controller.fsm.Change(controller.knockbackState, "Knockback started");
                }
            }
        }
        
        private class AirborneState : IState
        {
            private readonly PhysicsLocomotionController controller;
            
            public AirborneState(PhysicsLocomotionController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter() { }
            
            public void Exit() { }
            
            public void Tick(float dt)
            {
                // Check for jump input (double jump)
                if (controller.jumpBufferTimer > 0f)
                {
                    controller.TryJump();
                }
                
                // Check if we've landed
                if (controller.IsGrounded)
                {
                    controller.fsm.Change(controller.groundedState, "Landed");
                }
                
                // Check for knockback
                if (controller.IsInKnockback)
                {
                    controller.fsm.Change(controller.knockbackState, "Knockback started");
                }
            }
        }
        
        private class KnockbackState : IState
        {
            private readonly PhysicsLocomotionController controller;
            
            public KnockbackState(PhysicsLocomotionController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter() { }
            
            public void Exit()
            {
                controller.OnKnockbackEnd?.Invoke();
            }
            
            public void Tick(float dt)
            {
                // Check if knockback has ended
                if (!controller.IsInKnockback)
                {
                    // Return to appropriate state
                    IState nextState = controller.IsGrounded ? 
                        (IState)controller.groundedState : 
                        controller.airborneState;
                    controller.fsm.Change(nextState, "Knockback ended");
                }
            }
        }
        
        private class DisabledState : IState
        {
            private readonly PhysicsLocomotionController controller;
            
            public DisabledState(PhysicsLocomotionController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter()
            {
                // Stop movement
                controller.desiredVelocity = Vector3.zero;
            }
            
            public void Exit() { }
            
            public void Tick(float dt)
            {
                // Don't process input while disabled
                // External code must call SetEnabled(true) to exit this state
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (physicsSystem != null && !string.IsNullOrEmpty(physicsBodyId))
            {
                physicsSystem.UnregisterBody(physicsBodyId);
            }
        }
    }
}
