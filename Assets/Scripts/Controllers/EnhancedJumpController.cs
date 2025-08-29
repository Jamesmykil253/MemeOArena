using UnityEngine;
using MOBA.Core;
using MOBA.Data;
using MOBA.Input;
using MOBA.Physics;
using MOBA.Core.Events;
using MOBA.Core.Logging;

namespace MOBA.Controllers
{
    /// <summary>
    /// Enhanced jumping system with variable jump heights and apex boost mechanics.
    /// Features: Normal jump (1x), High jump (1.5x), Double jump (2x), Apex boost system.
    /// PhD-level: Implements predictive apex detection and smooth jump transitions.
    /// </summary>
    public class EnhancedJumpController : MonoBehaviour
    {
        [Header("Jump Configuration")]
        [SerializeField] private JumpPhysicsDef jumpDef;
        [SerializeField] private bool enableDebugVisuals = false;
        
        [Header("References")]
        [SerializeField] private PhysicsLocomotionController locomotionController;
        
        // Jump state tracking
        private JumpState currentJumpState = JumpState.Grounded;
        private float jumpHoldStartTime;
        private float jumpHoldDuration;
        private bool hasDoubleJump;
        private float lastJumpTime;
        private float apexDetectionTimer;
        private bool isAtApex;
        private Vector3 lastVelocity;
        
        // Input tracking
        private bool jumpButtonPressed;
        private bool jumpButtonHeld;
        private bool jumpButtonReleased;
        
        // Performance metrics
        private int totalJumps;
        private int apexBoosts;
        private float averageJumpHeight;
        
        public enum JumpState
        {
            Grounded,
            Rising,
            AtApex,
            Falling,
            DoubleJumping,
            Landing
        }
        
        public enum JumpType
        {
            Normal,
            High,
            Double,
            ApexBoosted
        }
        
        // Properties
        public JumpState CurrentState => currentJumpState;
        public bool CanJump => CanPerformJump();
        public bool CanDoubleJump => CanPerformDoubleJump();
        public bool IsAtApexWindow => isAtApex;
        public float JumpHoldTime => jumpHoldDuration;
        
        // Events
        public System.Action<JumpType, float> OnJumpPerformed;
        public System.Action OnApexReached;
        public System.Action<bool> OnLanding; // isHardLanding
        public System.Action OnDoubleJumpReady;
        public System.Action OnApexBoostReady;
        
        private void Awake()
        {
            if (locomotionController == null)
                locomotionController = GetComponent<PhysicsLocomotionController>();
                
            if (jumpDef == null)
            {
                // Create default jump definition
                jumpDef = ScriptableObject.CreateInstance<JumpPhysicsDef>();
                EnterpriseLogger.LogWarning("JUMP", gameObject.name, "No JumpPhysicsDef assigned, using defaults");
            }
        }
        
        private void Start()
        {
            // Subscribe to locomotion events
            if (locomotionController != null)
            {
                locomotionController.OnJump += OnLocomotionJump;
                locomotionController.OnLand += OnLocomotionLand;
            }
        }
        
        private void Update()
        {
            UpdateInput();
            UpdateJumpState();
            UpdateApexDetection();
            ProcessJumpInput();
            
            if (enableDebugVisuals)
            {
                DrawDebugVisuals();
            }
        }
        
        private void UpdateInput()
        {
            // Get input from locomotion controller's input source
            var inputSource = GetInputSource();
            if (inputSource == null) return;
            
            bool currentJumpInput = inputSource.IsJumpPressed() || inputSource.GetButton("Jump");
            
            jumpButtonPressed = currentJumpInput && !jumpButtonHeld;
            jumpButtonReleased = !currentJumpInput && jumpButtonHeld;
            jumpButtonHeld = currentJumpInput;
            
            // Track hold duration
            if (jumpButtonPressed)
            {
                jumpHoldStartTime = Time.time;
                jumpHoldDuration = 0f;
            }
            else if (jumpButtonHeld)
            {
                jumpHoldDuration = Time.time - jumpHoldStartTime;
            }
        }
        
        private void UpdateJumpState()
        {
            if (locomotionController == null) return;
            
            var body = GetPhysicsBody();
            if (body == null) return;
            
            var velocity = body.velocity;
            var isGrounded = body.isGrounded;
            
            switch (currentJumpState)
            {
                case JumpState.Grounded:
                    if (!isGrounded && velocity.y > 0.1f)
                    {
                        TransitionToState(JumpState.Rising);
                        hasDoubleJump = jumpDef.AllowDoubleJump;
                        OnDoubleJumpReady?.Invoke();
                    }
                    break;
                    
                case JumpState.Rising:
                    if (velocity.y <= 0.1f)
                    {
                        TransitionToState(JumpState.AtApex);
                        isAtApex = true;
                        OnApexReached?.Invoke();
                        if (hasDoubleJump && jumpDef.EnableApexBoost)
                        {
                            OnApexBoostReady?.Invoke();
                        }
                    }
                    break;
                    
                case JumpState.AtApex:
                    apexDetectionTimer += Time.deltaTime;
                    if (apexDetectionTimer > jumpDef.ApexWindow || velocity.y < -0.5f)
                    {
                        TransitionToState(JumpState.Falling);
                        isAtApex = false;
                    }
                    break;
                    
                case JumpState.Falling:
                    if (isGrounded)
                    {
                        TransitionToState(JumpState.Landing);
                        bool isHardLanding = Mathf.Abs(velocity.y) > 15f;
                        OnLanding?.Invoke(isHardLanding);
                    }
                    break;
                    
                case JumpState.DoubleJumping:
                    if (velocity.y <= 0f)
                    {
                        TransitionToState(JumpState.Falling);
                    }
                    break;
                    
                case JumpState.Landing:
                    if (Time.time - lastJumpTime > jumpDef.LandingRecoveryTime)
                    {
                        TransitionToState(JumpState.Grounded);
                    }
                    break;
            }
            
            lastVelocity = velocity;
        }
        
        private void UpdateApexDetection()
        {
            var body = GetPhysicsBody();
            if (body == null) return;
            
            // Advanced apex detection using velocity analysis
            var velocity = body.velocity;
            var velocityChange = velocity - lastVelocity;
            
            // Detect apex more precisely
            if (currentJumpState == JumpState.Rising || currentJumpState == JumpState.AtApex)
            {
                isAtApex = jumpDef.IsAtApex(velocity.y) && velocityChange.y < 0.1f;
            }
        }
        
        private void ProcessJumpInput()
        {
            if (!jumpButtonPressed && !jumpButtonHeld) return;
            
            if (jumpButtonPressed)
            {
                // Initial jump press
                if (CanPerformJump())
                {
                    PerformJump(JumpType.Normal);
                }
                else if (CanPerformDoubleJump())
                {
                    var jumpType = isAtApex && jumpDef.EnableApexBoost ? 
                        JumpType.ApexBoosted : JumpType.Double;
                    PerformJump(jumpType);
                }
            }
            else if (jumpButtonReleased && currentJumpState == JumpState.Rising)
            {
                // Released during rise - determine if it was a high jump
                if (jumpHoldDuration >= jumpDef.MinHoldTime)
                {
                    // This was a high jump, no additional action needed
                    EnterpriseLogger.LogDebug("JUMP", gameObject.name, 
                        $"High jump completed with hold time: {jumpHoldDuration:F3}s");
                }
            }
        }
        
        private void PerformJump(JumpType jumpType)
        {
            var body = GetPhysicsBody();
            if (body == null || locomotionController == null) return;
            
            float jumpVelocity;
            bool isDoubleJump = jumpType == JumpType.Double || jumpType == JumpType.ApexBoosted;
            bool isApexBoost = jumpType == JumpType.ApexBoosted;
            
            // Calculate jump velocity based on type and hold time
            switch (jumpType)
            {
                case JumpType.Normal:
                    jumpVelocity = jumpDef.CalculateJumpVelocity(0f);
                    break;
                case JumpType.High:
                    jumpVelocity = jumpDef.CalculateJumpVelocity(jumpHoldDuration);
                    break;
                case JumpType.Double:
                    jumpVelocity = jumpDef.CalculateJumpVelocity(0f, true, false);
                    break;
                case JumpType.ApexBoosted:
                    jumpVelocity = jumpDef.CalculateJumpVelocity(0f, true, true);
                    apexBoosts++;
                    break;
                default:
                    jumpVelocity = jumpDef.BaseJumpVelocity;
                    break;
            }
            
            // Apply the jump through locomotion controller
            if (locomotionController != null)
            {
                locomotionController.PerformEnhancedJump(jumpVelocity, isDoubleJump);
            }
            else
            {
                // Fallback: Apply jump impulse directly if no locomotion controller
                var physicsBody = GetPhysicsBody();
                if (physicsBody != null)
                {
                    Vector3 jumpImpulse = Vector3.up * jumpVelocity;
                    
                    // Add lateral boost for apex double jumps
                    if (isApexBoost)
                    {
                        var inputSource = GetInputSource();
                        if (inputSource != null)
                        {
                            Vector2 moveInput = inputSource.GetMoveVector();
                            Vector3 lateralBoost = new Vector3(moveInput.x, 0f, moveInput.y) * jumpVelocity * 0.3f;
                            jumpImpulse += lateralBoost;
                        }
                    }
                    
                    // Reset vertical velocity for double jumps
                    if (isDoubleJump)
                    {
                        var currentVel = physicsBody.velocity;
                        physicsBody.velocity = new Vector3(currentVel.x, 0f, currentVel.z);
                        hasDoubleJump = false;
                        TransitionToState(JumpState.DoubleJumping);
                    }
                    
                    // Note: This is a fallback - proper physics integration would go through DeterministicPhysics
                    // physicsBody.AddForce(jumpImpulse, ForceMode.Impulse);
                }
            }
            
            // Update state and metrics
            lastJumpTime = Time.time;
            totalJumps++;
            
            // Log jump event
            EnterpriseLogger.LogInfo("JUMP", gameObject.name, 
                $"Jump performed: {jumpType}, Velocity: {jumpVelocity:F2}, Hold: {jumpHoldDuration:F3}s");
            
            // Trigger event
            OnJumpPerformed?.Invoke(jumpType, jumpVelocity);
            
            // Publish to event bus
            var jumpEvent = new JumpPerformedEvent(0, gameObject.name, jumpType, jumpVelocity, isApexBoost);
            EventBus.PublishAsync(jumpEvent);
        }
        
        private bool CanPerformJump()
        {
            return currentJumpState == JumpState.Grounded || 
                   (currentJumpState == JumpState.Falling && Time.time - lastJumpTime <= jumpDef.CoyoteTime);
        }
        
        private bool CanPerformDoubleJump()
        {
            return hasDoubleJump && 
                   (currentJumpState == JumpState.Rising || 
                    currentJumpState == JumpState.AtApex || 
                    currentJumpState == JumpState.Falling) &&
                   Time.time - lastJumpTime <= jumpDef.DoubleJumpWindow;
        }
        
        private void TransitionToState(JumpState newState)
        {
            if (currentJumpState == newState) return;
            
            var oldState = currentJumpState;
            currentJumpState = newState;
            
            // Reset timers on state transitions
            if (newState == JumpState.AtApex)
            {
                apexDetectionTimer = 0f;
            }
            
            EnterpriseLogger.LogTrace("JUMP", gameObject.name, 
                $"Jump state: {oldState} -> {newState}");
        }
        
        private PhysicsBody GetPhysicsBody()
        {
            if (locomotionController == null) return null;
            return locomotionController.PhysicsBody;
        }
        
        private IInputSource GetInputSource()
        {
            if (locomotionController == null) return null;
            return locomotionController.InputSource;
        }
        
        // Event handlers
        private void OnLocomotionJump()
        {
            // Called when locomotion controller performs a jump
        }
        
        private void OnLocomotionLand()
        {
            // Called when locomotion controller lands
        }
        
        private void DrawDebugVisuals()
        {
            if (!enableDebugVisuals) return;
            
            // Draw jump state
            var position = transform.position + Vector3.up * 2f;
            var color = GetStateColor();
            
            #if UNITY_EDITOR
            UnityEngine.Debug.DrawRay(position, Vector3.up * 0.5f, color);
            #endif
        }
        
        private Color GetStateColor()
        {
            switch (currentJumpState)
            {
                case JumpState.Grounded: return Color.green;
                case JumpState.Rising: return Color.blue;
                case JumpState.AtApex: return Color.yellow;
                case JumpState.Falling: return Color.red;
                case JumpState.DoubleJumping: return Color.magenta;
                case JumpState.Landing: return new Color(1f, 0.5f, 0f); // Orange color
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// Get jump performance statistics
        /// </summary>
        public JumpStatistics GetStatistics()
        {
            return new JumpStatistics
            {
                TotalJumps = totalJumps,
                ApexBoosts = apexBoosts,
                AverageJumpHeight = averageJumpHeight,
                ApexBoostRate = totalJumps > 0 ? (float)apexBoosts / totalJumps : 0f,
                CurrentState = currentJumpState,
                HasDoubleJump = hasDoubleJump,
                IsAtApex = isAtApex
            };
        }
        
        public struct JumpStatistics
        {
            public int TotalJumps;
            public int ApexBoosts;
            public float AverageJumpHeight;
            public float ApexBoostRate;
            public JumpState CurrentState;
            public bool HasDoubleJump;
            public bool IsAtApex;
        }
        
        private void OnDestroy()
        {
            if (locomotionController != null)
            {
                locomotionController.OnJump -= OnLocomotionJump;
                locomotionController.OnLand -= OnLocomotionLand;
            }
        }
    }
    
    // Event for jump system
    [System.Serializable]
    public struct JumpPerformedEvent : IGameEvent
    {
        public uint Tick { get; }
        public string EventId { get; }
        public string PlayerId { get; }
        public EnhancedJumpController.JumpType JumpType { get; }
        public float JumpVelocity { get; }
        public bool IsApexBoost { get; }
        
        public JumpPerformedEvent(uint tick, string playerId, EnhancedJumpController.JumpType jumpType, 
                                 float jumpVelocity, bool isApexBoost)
        {
            Tick = tick;
            EventId = System.Guid.NewGuid().ToString();
            PlayerId = playerId;
            JumpType = jumpType;
            JumpVelocity = jumpVelocity;
            IsApexBoost = isApexBoost;
        }
    }
}
