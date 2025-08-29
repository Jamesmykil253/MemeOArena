using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Enhanced jump physics parameters with variable jump heights and apex boost mechanics.
    /// Supports: Normal jump (1x), High jump (1.5x), Double jump (2x), Apex boost system.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/JumpPhysicsDef", fileName = "JumpPhysicsDef", order = 2)]
    public class JumpPhysicsDef : ScriptableObject
    {
        [Header("Base Jump Settings")]
        public float BaseJumpVelocity = 10f;
        public float Gravity = -20f;
        public float CoyoteTime = 0.1f; // time after leaving ground when jump is still allowed
        
        [Header("Variable Jump Heights")]
        public float NormalJumpMultiplier = 1.0f;     // Base jump
        public float HighJumpMultiplier = 1.5f;       // Hold button for higher jump
        public float DoubleJumpMultiplier = 2.0f;     // Double jump power
        
        [Header("High Jump Hold Mechanics")]
        public float MinHoldTime = 0.1f;              // Minimum hold for high jump
        public float MaxHoldTime = 0.4f;              // Maximum effective hold time
        public AnimationCurve JumpHoldCurve = new AnimationCurve(
            new Keyframe(0f, 1f), 
            new Keyframe(1f, 1.5f)
        );
        
        [Header("Double Jump Settings")]
        public float DoubleJumpWindow = 0.5f;         // window after first jump where double jump is allowed
        public bool AllowDoubleJump = true;
        
        [Header("Apex Boost System")]
        public bool EnableApexBoost = true;
        public float ApexDetectionThreshold = 0.5f;   // Velocity threshold to detect apex
        public float ApexBoostMultiplier = 1.8f;      // Boost power when double jumping at apex
        public float ApexWindow = 0.15f;              // Time window around apex for boost
        public Vector3 ApexBoostDirection = new Vector3(0f, 1f, 0f); // Boost direction
        
        [Header("Air Control")]
        public float AirControlMultiplier = 0.6f;     // Reduced movement control while airborne
        public float MaxFallSpeed = -25f;             // Terminal velocity
        
        [Header("Advanced Mechanics")]
        public float JumpBufferTime = 0.1f;           // Input buffer for responsive jumping
        public bool AllowWallJump = false;            // Future: wall jumping
        public float LandingRecoveryTime = 0.05f;     // Brief period after landing before next jump
        
        /// <summary>
        /// Calculate jump velocity based on hold time
        /// </summary>
        public float CalculateJumpVelocity(float holdTime, bool isDoubleJump = false, bool isApexBoost = false)
        {
            if (isDoubleJump)
            {
                float multiplier = isApexBoost ? DoubleJumpMultiplier * ApexBoostMultiplier : DoubleJumpMultiplier;
                return BaseJumpVelocity * multiplier;
            }
            
            // Clamp hold time to valid range
            holdTime = Mathf.Clamp(holdTime, 0f, MaxHoldTime);
            
            // Use normal jump for very short holds
            if (holdTime < MinHoldTime)
            {
                return BaseJumpVelocity * NormalJumpMultiplier;
            }
            
            // Calculate high jump based on hold curve
            float normalizedHoldTime = holdTime / MaxHoldTime;
            float curveValue = JumpHoldCurve.Evaluate(normalizedHoldTime);
            return BaseJumpVelocity * curveValue;
        }
        
        /// <summary>
        /// Check if velocity indicates player is at jump apex
        /// </summary>
        public bool IsAtApex(float verticalVelocity)
        {
            return Mathf.Abs(verticalVelocity) <= ApexDetectionThreshold;
        }
        
        /// <summary>
        /// Get effective air control based on current state
        /// </summary>
        public float GetAirControlFactor(bool isDoubleJumping = false)
        {
            return isDoubleJumping ? AirControlMultiplier * 1.2f : AirControlMultiplier;
        }
    }
}