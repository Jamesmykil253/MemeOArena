using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Spawn
{
    /// <summary>
    /// Handles deterministic player spawning using a finite state machine.  Each
    /// state performs a portion of the spawn pipeline and signals completion
    /// events to transition to the next state.  On any failure, the machine
    /// enters an error state and cleans up.
    /// </summary>
    public class SpawnMachine : StateMachine
    {
        private readonly PlayerContext ctx;
        private readonly IdleState idle;
        private readonly InitialSetupState setup;
        private readonly AssignBaseStatsState assign;
        private readonly ValidateStatsState validate;
        private readonly FinalizeSpawnState finalize;
        private readonly ErrorState error;

        public SpawnMachine(PlayerContext context)
        {
            ctx = context;
            idle = new IdleState(this);
            setup = new InitialSetupState(this);
            assign = new AssignBaseStatsState(this);
            validate = new ValidateStatsState(this);
            finalize = new FinalizeSpawnState(this);
            error = new ErrorState(this);
            Change(idle);
        }

        #region External Events
        public void SpawnRequest()
        {
            Change(setup);
        }
        public void SetupComplete()
        {
            Change(assign);
        }
        public void SetupError()
        {
            Change(error);
        }
        public void StatsAssigned()
        {
            Change(validate);
        }
        public void AssignmentError()
        {
            Change(error);
        }
        public void ValidationOK()
        {
            Change(finalize);
        }
        public void ValidationFailure()
        {
            Change(error);
        }
        public void FinalizationOK()
        {
            // Player is now fully spawned; no further state inside the machine
        }
        public void FinalizationError()
        {
            Change(error);
        }
        public void Retry()
        {
            Change(idle);
        }
        #endregion

        #region State Definitions
        private abstract class SpawnState : IState
        {
            protected readonly SpawnMachine machine;
            public SpawnState(SpawnMachine m) { machine = m; }
            public virtual void Enter() { }
            public virtual void Exit() { }
            public abstract void Tick(float dt);
        }

        private class IdleState : SpawnState
        {
            public IdleState(SpawnMachine m) : base(m) { }
            public override void Tick(float dt) { }
        }

        private class InitialSetupState : SpawnState
        {
            public InitialSetupState(SpawnMachine m) : base(m) { }
            public override void Enter()
            {
                // Instantiate player root and network object.  In a real game this
                // would involve loading a prefab and registering with the network.
                bool success = true; // placeholder
                if (success)
                {
                    machine.SetupComplete();
                }
                else
                {
                    machine.SetupError();
                }
            }
            public override void Tick(float dt) { }
        }

        private class AssignBaseStatsState : SpawnState
        {
            public AssignBaseStatsState(SpawnMachine m) : base(m) { }
            public override void Enter()
            {
                // Copy base stats into runtime context.  If template missing, error.
                if (machine.ctx.baseStats == null)
                {
                    machine.AssignmentError();
                    return;
                }
                machine.ctx.ResetToTemplate();
                // Copy ability definitions; ensure list is not empty
                if (machine.ctx.abilities == null || machine.ctx.abilities.Count == 0)
                {
                    // For demonstration we allow empty ability lists
                }
                machine.StatsAssigned();
            }
            public override void Tick(float dt) { }
        }

        private class ValidateStatsState : SpawnState
        {
            public ValidateStatsState(SpawnMachine m) : base(m) { }
            public override void Enter()
            {
                // Clamp ranges; compute derived stats; apply mode modifiers
                if (machine.ctx.currentHP <= 0f || float.IsNaN(machine.ctx.currentHP))
                {
                    machine.ValidationFailure();
                    return;
                }
                // Example derived stat: adjust move speed based on level
                machine.ctx.moveSpeed = machine.ctx.baseStats.MoveSpeed * (1f + (machine.ctx.level - 1) * 0.05f);
                machine.ValidationOK();
            }
            public override void Tick(float dt) { }
        }

        private class FinalizeSpawnState : SpawnState
        {
            public FinalizeSpawnState(SpawnMachine m) : base(m) { }
            public override void Enter()
            {
                // Register with subsystems (physics, AI, scoreboard).  In this demo we simply succeed.
                bool success = true;
                if (success)
                {
                    machine.FinalizationOK();
                }
                else
                {
                    machine.FinalizationError();
                }
            }
            public override void Tick(float dt) { }
        }

        private class ErrorState : SpawnState
        {
            public ErrorState(SpawnMachine m) : base(m) { }
            public override void Enter()
            {
                // Log error and clean up resources.  For demonstration we do nothing.
                // Automatically retry could be triggered externally by calling Retry().
            }
            public override void Tick(float dt) { }
        }
        #endregion
    }
}