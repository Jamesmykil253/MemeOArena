using System;
using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Controllers
{
    /// <summary>
    /// Standardized interface for all locomotion controllers.
    /// Provides unified API for movement, jumping, and state management.
    /// AAA PhD-Level Architecture: Single contract for all locomotion implementations.
    /// </summary>
    public interface ILocomotionController
    {
        /// <summary>
        /// Initialize the controller with player context and input source
        /// </summary>
        void Initialize(PlayerContext playerContext, IInputSource inputSource);
        
        /// <summary>
        /// Update the controller logic (called per frame or per tick)
        /// </summary>
        void Tick(float deltaTime);
        
        /// <summary>
        /// Current desired velocity for physics integration
        /// </summary>
        Vector3 DesiredVelocity { get; }
        
        /// <summary>
        /// Whether the character is currently grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Current movement speed (can be modified by abilities/effects)
        /// </summary>
        float MoveSpeed { get; set; }
        
        /// <summary>
        /// Enable/disable locomotion (for abilities, stunts, etc.)
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Attempt to perform a jump
        /// </summary>
        void TryJump();
        
        /// <summary>
        /// Apply external knockback force
        /// </summary>
        void ApplyKnockback(Vector3 direction, float force, float duration);
        
        /// <summary>
        /// Events for state changes
        /// </summary>
        event Action OnJump;
        event Action OnLand;
        event Action<Vector3> OnKnockbackStart;
        event Action OnKnockbackEnd;
    }
    
    /// <summary>
    /// Locomotion state enumeration for external systems
    /// </summary>
    public enum LocomotionState
    {
        Grounded,
        Airborne,
        Knockback,
        Disabled
    }
}
