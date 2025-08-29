using System;
using UnityEngine;
using MOBA.Core;
using MOBA.Data;
using MOBA.Physics;

namespace MOBA.Controllers
{
    /// <summary>
    /// Unified locomotion controller that can work with or without physics integration.
    /// Implements the full FSM: Grounded ↔ Airborne with Knockback/Disabled states.
    /// Falls back to simple movement when physics system is not available.
    /// </summary>
    public class UnifiedLocomotionController : MonoBehaviour
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
        
        [Header("Integration")]
        [SerializeField] private bool usePhysicsIntegration = true;
        [SerializeField] private bool useFSMStates = true;
        
        // External references
        private DeterministicPhysics physicsSystem;
        private IInputSource inputSource;
        private StateMachine fsm;
        private TickManager tickManager;
        
        // States (only used if useFSMStates is true)
        private GroundedState groundedState;
        private AirborneState airborneState;
        private KnockbackState knockbackState;
        private DisabledState disabledState;
        
        // Runtime data
        private PlayerContext context;
        private string physicsBodyId;
        private Vector3 desiredVelocity;
        private float lastGroundedTime;
        private float jumpBufferTimer;
        private bool hasDoubleJump;
        private bool isEnabled = true;
        
        // Simple mode properties (when not using physics/FSM)
        private Vector3 simpleVelocity;
        private bool isSimpleGrounded = true;
        
        // Public properties
        public Vector3 DesiredVelocity => desiredVelocity;
        public bool IsGrounded => usePhysicsIntegration ? 
            (physicsSystem?.GetBody(physicsBodyId)?.isGrounded ?? isSimpleGrounded) : 
            isSimpleGrounded;
        public bool IsInKnockback => usePhysicsIntegration ? 
            (physicsSystem?.GetBody(physicsBodyId)?.isInKnockback ?? false) : 
            false;
        public StateMachine FSM => fsm;
        public bool IsPhysicsEnabled => usePhysicsIntegration && physicsSystem != null;
        public bool IsFSMEnabled => useFSMStates && fsm != null;
        
        // Events
        public event Action OnJump;
        public event Action OnLand;
        public event Action<Vector3> OnKnockbackStart;
        public event Action OnKnockbackEnd;
        
        private void Awake()
        {
            // Try to find physics system and tick manager
            if (usePhysicsIntegration)
            {
                physicsSystem = FindFirstObjectByType<DeterministicPhysics>();
                tickManager = FindFirstObjectByType<TickManager>();
                
                if (physicsSystem == null)
                {
                    Debug.LogWarning("UnifiedLocomotionController: DeterministicPhysics not found, using simple movement");
                    usePhysicsIntegration = false;
                }
                
                if (tickManager == null)
                {
                    Debug.LogWarning("UnifiedLocomotionController: TickManager not found");
                }
            }
            
            // Setup FSM if enabled
            if (useFSMStates)
            {
                SetupStateMachine();
            }
        }
        
        private void Start()
        {
            // Register physics body if using physics
            if (usePhysicsIntegration && physicsSystem != null)
            {
                RegisterPhysicsBody();
            }
            
            // Start FSM if enabled
            if (useFSMStates && fsm != null && groundedState != null)
            {
                fsm.Change(groundedState, "Initial state");
            }
        }
        
        /// <summary>
        /// Initialize the controller with context and input source
        /// </summary>
        public void Initialize(PlayerContext playerContext, IInputSource input)
        {
            context = playerContext;
            inputSource = input;
            
            if (playerContext != null)
            {
                physicsBodyId = playerContext.playerId + "_body";
                
                // Update move speed from context
                if (playerContext.baseStats != null)
                {
                    moveSpeed = playerContext.baseStats.MoveSpeed;
                }
            }
        }
        
        private void SetupStateMachine()
        {
            if (context != null)
            {
                fsm = new StateMachine("LocomotionFSM", context.playerId);
            }
            else
            {
                fsm = new StateMachine("LocomotionFSM", "unknown");
            }
            
            // Create states
            groundedState = new GroundedState(this);
            airborneState = new AirborneState(this);
            knockbackState = new KnockbackState(this);
            disabledState = new DisabledState(this);
        }
        
        private void RegisterPhysicsBody()
        {
            if (physicsSystem == null || string.IsNullOrEmpty(physicsBodyId)) 
            {
                return;
            }
            
            PhysicsBodySettings settings = PhysicsBodySettings.Default;
            settings.mass = 1f;
            settings.useGravity = true;
            
            physicsSystem.RegisterBody(physicsBodyId, transform.position, Vector3.zero, settings);
        }
        
        public void Update()
        {
            Tick(Time.deltaTime);
        }
        
        public void Tick(float dt)
        {
            // Update timers
            UpdateTimers(dt);
            
            // Process input
            ProcessInput();
            
            // Update FSM if enabled
            if (useFSMStates && fsm != null)
            {
                fsm.Update(dt);
            }
            
            // Apply movement
            ApplyMovement(dt);
            
            // Sync transform
            if (usePhysicsIntegration)
            {
                SyncTransformWithPhysics();
            }
        }
        
        /// <summary>
        /// Public method for external update calls (matches simple LocomotionController interface)
        /// This allows the controller to be used without MonoBehaviour if needed
        /// </summary>
        public void UpdateController(float dt)
        {
            Tick(dt);
        }
        
        /// <summary>
        /// Compatibility method for existing LocomotionController interface
        /// </summary>
        public void UpdateController()
        {
            UpdateController(Time.deltaTime);
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
            if (inputSource == null || !isEnabled) 
            {
                desiredVelocity = Vector3.zero;
                return;
            }
            
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
        
        private void ApplyMovement(float dt)
        {
            if (!isEnabled) return;
            
            if (usePhysicsIntegration && physicsSystem != null)
            {
                ApplyPhysicsMovement();
            }
            else
            {
                ApplySimpleMovement(dt);
            }
        }
        
        private void ApplyPhysicsMovement()
        {
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
        
        private void ApplySimpleMovement(float dt)
        {
            // Simple movement (fallback when physics not available)
            if (desiredVelocity.magnitude > 0.01f)
            {
                // Apply movement directly to transform
                transform.position += desiredVelocity * dt;
                
                // Rotate to face movement direction
                Vector3 lookDirection = desiredVelocity.normalized;
                lookDirection.y = 0; // Keep on horizontal plane
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, 
                                                       Quaternion.LookRotation(lookDirection), 
                                                       10f * dt);
                }
            }
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
            float jumpVelocity = hasDoubleJump && !IsGrounded ? doubleJumpForce : jumpForce;
            
            if (usePhysicsIntegration && physicsSystem != null)
            {
                Vector3 jumpImpulse = Vector3.up * jumpVelocity;
                physicsSystem.ApplyImpulse(physicsBodyId, jumpImpulse);
            }
            else
            {
                // Simple jump - just set upward velocity
                simpleVelocity.y = jumpVelocity;
                isSimpleGrounded = false;
            }
            
            jumpBufferTimer = 0f;
            OnJump?.Invoke();
            
            if (useFSMStates && fsm != null && airborneState != null)
            {
                fsm.Change(airborneState, "Jump");
            }
        }
        
        /// <summary>
        /// Apply knockback force
        /// </summary>
        public void ApplyKnockback(Vector3 direction, float force, float duration)
        {
            if (usePhysicsIntegration && physicsSystem != null)
            {
                physicsSystem.ApplyKnockback(physicsBodyId, direction, force, duration);
            }
            
            if (useFSMStates && fsm != null && knockbackState != null)
            {
                fsm.Change(knockbackState, "Knockback applied");
            }
            
            OnKnockbackStart?.Invoke(direction);
        }
        
        /// <summary>
        /// Enable/disable locomotion (for abilities, etc.)
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            
            if (useFSMStates && fsm != null)
            {
                if (enabled)
                {
                    IState nextState = IsGrounded ? 
                        (IState)groundedState : 
                        airborneState;
                    fsm.Change(nextState, "Enabled");
                }
                else if (disabledState != null)
                {
                    fsm.Change(disabledState, "Disabled");
                }
            }
        }
        
        #region State Classes
        
        private class GroundedState : IState
        {
            private readonly UnifiedLocomotionController controller;
            
            public GroundedState(UnifiedLocomotionController ctrl)
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
            private readonly UnifiedLocomotionController controller;
            
            public AirborneState(UnifiedLocomotionController ctrl)
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
            private readonly UnifiedLocomotionController controller;
            
            public KnockbackState(UnifiedLocomotionController ctrl)
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
            private readonly UnifiedLocomotionController controller;
            
            public DisabledState(UnifiedLocomotionController ctrl)
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
