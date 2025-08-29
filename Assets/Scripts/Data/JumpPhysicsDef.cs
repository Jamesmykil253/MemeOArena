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
        
        [Header("Variable Jump Heights - New Formula")]
        public float NormalJumpMultiplier = 1.0f;     // Press jump button (space)
        public float HighJumpMultiplier = 1.5f;       // Hold jump button  
        public float DoubleJumpMultiplier = 2.0f;     // Double jump press button
        public float ApexJumpMultiplier = 2.5f;       // Jump at max height (apex)
        public float ApexWindowJumpMultiplier = 2.8f; // Jump within height apex window
        
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
        
        [Header("Apex Boost System - Enhanced")]
        public bool EnableApexBoost = true;
        public float ApexDetectionThreshold = 0.5f;   // Velocity threshold to detect apex
        public float ApexBoostMultiplier = 1.8f;      // Legacy: kept for compatibility
        public float ApexWindow = 0.15f;              // Time window around apex for enhanced boost
        public Vector3 ApexBoostDirection = new Vector3(0f, 1f, 0f); // Boost direction
        
        [Header("Apex Timing Windows")]
        public float MaxHeightWindow = 0.05f;         // Tight window for 2.5x jump (exact apex)
        public float ApexRegionWindow = 0.25f;        // Wider window for 2.8x jump (apex region)
        
        [Header("Air Control")]
        public float AirControlMultiplier = 0.6f;     // Reduced movement control while airborne
        public float MaxFallSpeed = -25f;             // Terminal velocity
        
        [Header("Advanced Mechanics")]
        public float JumpBufferTime = 0.1f;           // Input buffer for responsive jumping
        public bool AllowWallJump = false;            // Future: wall jumping
        public float LandingRecoveryTime = 0.05f;     // Brief period after landing before next jump
        
        /// <summary>
        /// Calculate jump velocity based on the new jump formula:
        /// - Press jump (quick): 1.0x
        /// - Hold jump: 1.5x  
        /// - Double jump: 2.0x
        /// - Jump at max height (tight apex): 2.5x
        /// - Jump within apex region: 2.8x
        /// </summary>
        public float CalculateJumpVelocity(float holdTime, bool isDoubleJump = false, bool isApexBoost = false, ApexJumpType apexType = ApexJumpType.None)
        {
            if (isDoubleJump)
            {
                // Determine apex boost type for double jumps
                switch (apexType)
                {
                    case ApexJumpType.MaxHeight:
                        return BaseJumpVelocity * ApexJumpMultiplier; // 2.5x - Jump at max height
                    case ApexJumpType.ApexWindow:
                        return BaseJumpVelocity * ApexWindowJumpMultiplier; // 2.8x - Jump within apex region
                    default:
                        return BaseJumpVelocity * DoubleJumpMultiplier; // 2.0x - Standard double jump
                }
            }
            
            // Clamp hold time to valid range
            holdTime = Mathf.Clamp(holdTime, 0f, MaxHoldTime);
            
            // Use normal jump for quick press/release (under minimum hold time)
            if (holdTime < MinHoldTime)
            {
                return BaseJumpVelocity * NormalJumpMultiplier; // 1.0x - Press jump button
            }
            
            // High jump for held button
            return BaseJumpVelocity * HighJumpMultiplier; // 1.5x - Hold jump button
        }
        
        /// <summary>
        /// Apex jump types for the enhanced formula
        /// </summary>
        public enum ApexJumpType
        {
            None,       // Standard jump
            MaxHeight,  // Jump at exact apex (tight window) - 2.5x
            ApexWindow  // Jump within apex region (wider window) - 2.8x
        }
        
        /// <summary>
        /// Determine apex jump type based on timing and velocity
        /// </summary>
        public ApexJumpType GetApexJumpType(float verticalVelocity, float apexTime)
        {
            if (!IsAtApex(verticalVelocity))
                return ApexJumpType.None;
                
            // Tight window for exact max height (2.5x)
            if (apexTime <= MaxHeightWindow)
            {
                return ApexJumpType.MaxHeight;
            }
            
            // Wider window for apex region (2.8x) 
            if (apexTime <= ApexRegionWindow)
            {
                return ApexJumpType.ApexWindow;
            }
            
            return ApexJumpType.None;
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