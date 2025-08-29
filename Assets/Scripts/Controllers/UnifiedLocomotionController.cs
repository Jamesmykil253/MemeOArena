using System;
using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Controllers
{
    /// <summary>
    /// Unified locomotion controller that handles all player movement.
    /// Implements ILocomotionController for standardized movement API.
    /// Integrates with CharacterController for physics-based movement.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class UnifiedLocomotionController : MonoBehaviour, ILocomotionController
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float groundCheckDistance = 0.1f;
        
        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayer = 1; // Default layer
        [SerializeField] private Transform groundCheck;
        
        // Component references
        private CharacterController characterController;
        private IInputSource inputSource;
        private PlayerContext playerContext;
        
        // Movement state
        private Vector3 velocity;
        private Vector3 desiredVelocity;
        private bool isGrounded;
        private bool isEnabled = true;
        private float knockbackTimer;
        private Vector3 knockbackVelocity;
        
        // Events
        public event Action OnJump;
        public event Action OnLand;
        public event Action<Vector3> OnKnockbackStart;
        public event Action OnKnockbackEnd;
        
        // Properties
        public Vector3 DesiredVelocity => desiredVelocity;
        public bool IsGrounded => isGrounded;
        public float MoveSpeed 
        { 
            get => moveSpeed; 
            set => moveSpeed = value; 
        }
        public bool IsEnabled 
        { 
            get => isEnabled; 
            set => isEnabled = value; 
        }
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            // Create ground check if not assigned
            if (groundCheck == null)
            {
                var groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = Vector3.down * (characterController.height / 2f);
                groundCheck = groundCheckObj.transform;
            }
        }
        
        public void Initialize(PlayerContext playerContext, IInputSource inputSource)
        {
            this.playerContext = playerContext;
            this.inputSource = inputSource;
        }
        
        private void Update()
        {
            Tick(Time.deltaTime);
        }
        
        public void Tick(float deltaTime)
        {
            if (!isEnabled) return;
            
            UpdateGroundCheck();
            HandleMovementInput(deltaTime);
            HandleKnockback(deltaTime);
            ApplyMovement(deltaTime);
        }
        
        private void UpdateGroundCheck()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);
            
            // Landing event
            if (isGrounded && !wasGrounded)
            {
                OnLand?.Invoke();
            }
        }
        
        private void HandleMovementInput(float deltaTime)
        {
            if (inputSource == null) return;
            
            // Get movement input
            Vector2 moveInput = inputSource.GetMoveVector();
            
            // Convert to world space movement
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            
            // Apply camera-relative movement (if camera exists)
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                var cameraTransform = mainCamera.transform;
                var forward = cameraTransform.forward;
                var right = cameraTransform.right;
                
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();
                
                moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            }
            
            // Calculate desired velocity
            desiredVelocity = moveDirection * moveSpeed;
            
            // Apply horizontal movement
            velocity.x = desiredVelocity.x;
            velocity.z = desiredVelocity.z;
            
            // Handle jump input
            if (inputSource.IsJumpPressed())
            {
                TryJump();
            }
        }
        
        private void HandleKnockback(float deltaTime)
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= deltaTime;
                
                // Apply knockback velocity
                velocity = Vector3.Lerp(velocity, knockbackVelocity, deltaTime * 10f);
                
                if (knockbackTimer <= 0f)
                {
                    knockbackVelocity = Vector3.zero;
                    OnKnockbackEnd?.Invoke();
                }
            }
        }
        
        private void ApplyMovement(float deltaTime)
        {
            // Apply gravity
            if (!isGrounded)
            {
                velocity.y -= gravity * deltaTime;
            }
            else if (velocity.y < 0f)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            
            // Move the character
            characterController.Move(velocity * deltaTime);
        }
        
        public void TryJump()
        {
            if (!isEnabled || !isGrounded) return;
            
            velocity.y = jumpForce;
            OnJump?.Invoke();
        }
        
        public void ApplyKnockback(Vector3 direction, float force, float duration)
        {
            knockbackVelocity = direction.normalized * force;
            knockbackTimer = duration;
            OnKnockbackStart?.Invoke(direction);
        }
        
        private void OnDestroy()
        {
            // No input event cleanup needed since we poll input rather than subscribe to events
        }
        
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);
            }
            
            // Draw desired velocity
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + desiredVelocity);
            
            // Draw actual velocity
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
        }
    }
}
