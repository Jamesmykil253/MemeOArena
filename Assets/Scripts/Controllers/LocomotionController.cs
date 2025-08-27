using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Controllers
{
    /// <summary>
    /// Handles locomotion for a player using a finite state machine.  It reads
    /// movement and jump inputs, applies deterministic kinematics from
    /// JumpPhysicsDef and transitions between grounded, airborne, knockback and
    /// disabled states.  The controller does not move the character directly;
    /// instead it exposes desired velocity, allowing the physics system to
    /// apply movement at a fixed timestep.
    /// </summary>
    public class LocomotionController
    {
        private readonly PlayerContext ctx;
        private readonly StateMachine fsm = new StateMachine();
        private readonly JumpPhysicsDef jumpDef;

        // Runtime variables for jump physics
        private float verticalVelocity;
        private float coyoteTimer;
        private float doubleJumpTimer;
        private bool doubleJumpUsed;
        private bool isGrounded;

        public Vector3 DesiredVelocity { get; private set; }

        public LocomotionController(PlayerContext context)
        {
            ctx = context;
            jumpDef = context.baseStats.JumpPhysics;
            // Initialize states
            grounded = new GroundedState(this);
            airborne = new AirborneState(this);
            knockback = new KnockbackState(this);
            disabled = new DisabledState(this);
            fsm.Change(grounded);
        }

        // Define states as nested classes
        private readonly GroundedState grounded;
        private readonly AirborneState airborne;
        private readonly KnockbackState knockback;
        private readonly DisabledState disabled;

        public void Update(float dt)
        {
            fsm.Update(dt);
        }

        public void Knockback(Vector3 force)
        {
            // Enter knockback state and apply force
            knockback.SetKnockback(force);
            fsm.Change(knockback);
        }

        public void Disable(float duration)
        {
            disabled.SetDisabledDuration(duration);
            fsm.Change(disabled);
        }

        #region States
        private abstract class LocomotionState : IState
        {
            protected readonly LocomotionController controller;
            public LocomotionState(LocomotionController c) { controller = c; }
            public virtual void Enter() { }
            public virtual void Exit() { }
            public abstract void Tick(float dt);
        }

        private class GroundedState : LocomotionState
        {
            public GroundedState(LocomotionController c) : base(c) { }
            public override void Enter()
            {
                controller.isGrounded = true;
                controller.doubleJumpUsed = false;
                controller.verticalVelocity = 0f;
            }
            public override void Tick(float dt)
            {
                // Read input for horizontal movement
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                Vector3 move = new Vector3(h, 0f, v);
                move = Vector3.ClampMagnitude(move, 1f);
                controller.DesiredVelocity = move * controller.ctx.moveSpeed;

                // Jump input
                if (Input.GetButtonDown("Jump"))
                {
                    controller.verticalVelocity = controller.jumpDef.InitialVelocity;
                    controller.coyoteTimer = controller.jumpDef.CoyoteTime;
                    controller.doubleJumpTimer = controller.jumpDef.DoubleJumpWindow;
                    controller.fsm.Change(controller.airborne);
                }
            }
        }

        private class AirborneState : LocomotionState
        {
            public AirborneState(LocomotionController c) : base(c) { }
            public override void Tick(float dt)
            {
                // Apply gravity
                controller.verticalVelocity += controller.jumpDef.Gravity * dt;
                // Move horizontally based on input
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                Vector3 move = new Vector3(h, 0f, v);
                move = Vector3.ClampMagnitude(move, 1f);
                controller.DesiredVelocity = move * controller.ctx.moveSpeed;

                // Jump again if allowed (double jump)
                if (Input.GetButtonDown("Jump") && !controller.doubleJumpUsed && controller.doubleJumpTimer > 0f)
                {
                    controller.verticalVelocity = controller.jumpDef.InitialVelocity;
                    controller.doubleJumpUsed = true;
                }
                controller.doubleJumpTimer -= dt;

                // Coyote time expired?  If vertical velocity is falling and we detect ground, transition to grounded.
                controller.coyoteTimer -= dt;
                if (controller.verticalVelocity <= 0f && controller.IsGroundDetected())
                {
                    controller.fsm.Change(controller.grounded);
                    return;
                }
            }
        }

        private class KnockbackState : LocomotionState
        {
            private Vector3 knockbackVelocity;
            private float timer;

            public KnockbackState(LocomotionController c) : base(c) { }
            public void SetKnockback(Vector3 force)
            {
                knockbackVelocity = force;
                timer = 0.3f; // duration of knockback
            }
            public override void Enter()
            {
                controller.DesiredVelocity = knockbackVelocity;
            }
            public override void Tick(float dt)
            {
                timer -= dt;
                if (timer <= 0f)
                {
                    controller.fsm.Change(controller.grounded);
                }
            }
        }

        private class DisabledState : LocomotionState
        {
            private float disableTimer;
            public DisabledState(LocomotionController c) : base(c) { }
            public void SetDisabledDuration(float d)
            {
                disableTimer = d;
            }
            public override void Enter()
            {
                controller.DesiredVelocity = Vector3.zero;
            }
            public override void Tick(float dt)
            {
                disableTimer -= dt;
                controller.DesiredVelocity = Vector3.zero;
                if (disableTimer <= 0f)
                {
                    // Return to grounded when recovered
                    controller.fsm.Change(controller.grounded);
                }
            }
        }
        #endregion

        /// <summary>
        /// Detect if the player has landed.  In a real implementation this
        /// would perform a collision check against the environment.  Here it
        /// simply uses Unity's CharacterController or a flag updated by
        /// collision callbacks.  For demonstration we assume the player is
        /// grounded when verticalVelocity is negative and near zero.
        /// </summary>
        private bool IsGroundDetected()
        {
            // Placeholder: In a real game this would call into a physics system
            // to detect ground contact.  Here we approximate by checking a
            // custom flag that could be set by collision detection.
            return isGrounded;
        }
    }
}