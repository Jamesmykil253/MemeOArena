using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Controllers
{
    /// <summary>
    /// Manages casting of abilities and ultimates using a finite state machine.
    /// Supports multiple abilities defined in the player's PlayerContext.
    /// Abilities transition through Idle → Casting → Executing → Cooldown.  An
    /// ultimate is simply an ability with a very high energy requirement.
    /// </summary>
    public class AbilityController
    {
        private readonly PlayerContext ctx;
        private readonly StateMachine fsm = new StateMachine();
        private IAbilityState idle;
        private IAbilityState casting;
        private IAbilityState executing;
        private IAbilityState cooldown;

        // Current ability being cast
        private AbilityDef currentAbility;
        private float timer;

        public AbilityController(PlayerContext context)
        {
            ctx = context;
            idle = new IdleState(this);
            casting = new CastingState(this);
            executing = new ExecutingState(this);
            cooldown = new CooldownState(this);
            fsm.Change(idle);
        }

        public void Update(float dt)
        {
            // Update energy regeneration
            RegenerateEnergy(dt);
            fsm.Update(dt);
        }

        private void RegenerateEnergy(float dt)
        {
            // Passive regen occurs only if not on cooldown
            if (ctx.ultimateCooldownRemaining > 0f)
            {
                ctx.ultimateCooldownRemaining -= dt;
                if (ctx.ultimateCooldownRemaining <= 0f)
                {
                    ctx.ultimateCooldownRemaining = 0f;
                }
                return;
            }
            ctx.ultimateEnergy += ctx.ultimateDef.passiveRegenPerSecond * dt;
            if (ctx.ultimateEnergy >= ctx.ultimateDef.energyRequirement)
            {
                ctx.ultimateReady = true;
            }
        }

        /// <summary>
        /// Attempt to cast an ability by index in the player's ability list.
        /// If an ultimate is attempted, ensure that enough energy is available.
        /// </summary>
        /// <param name="abilityIndex">Index into ctx.abilities.</param>
        public void TryCast(int abilityIndex)
        {
            if (abilityIndex < 0 || abilityIndex >= ctx.abilities.Count)
                return;
            AbilityDef ability = ctx.abilities[abilityIndex];
            // For ultimate we assume the last ability in the list is the ultimate
            bool isUltimate = (abilityIndex == ctx.abilities.Count - 1);
            if (isUltimate && !ctx.ultimateReady)
            {
                return;
            }
            // Cannot cast if not idle or if on cooldown
            if (fsm.Current != idle) return;
            // Start casting
            currentAbility = ability;
            timer = ability.CastTime;
            fsm.Change(casting);
        }

        #region State Definitions
        private interface IAbilityState : IState { }
        private class IdleState : IAbilityState
        {
            private readonly AbilityController ctrl;
            public IdleState(AbilityController c) { ctrl = c; }
            public void Enter() { }
            public void Exit() { }
            public void Tick(float dt)
            {
                // Wait for input.  Input is handled via TryCast.
            }
        }
        private class CastingState : IAbilityState
        {
            private readonly AbilityController ctrl;
            public CastingState(AbilityController c) { ctrl = c; }
            public void Enter() { }
            public void Exit() { }
            public void Tick(float dt)
            {
                ctrl.timer -= dt;
                // Cancel if player moved or took damage.  In a full implementation
                // this check would look at locomotion or damage flags.  Here we
                // simply allow the cast to complete.
                if (ctrl.timer <= 0f)
                {
                    ctrl.timer = ctrl.currentAbility.Cooldown;
                    ctrl.fsm.Change(ctrl.executing);
                }
            }
        }
        private class ExecutingState : IAbilityState
        {
            private readonly AbilityController ctrl;
            public ExecutingState(AbilityController c) { ctrl = c; }
            public void Enter()
            {
                // Apply the ability’s effect.  For demonstration we just deal
                // damage to a dummy target.  In a real game this would call
                // CombatSystem.ApplyAbility.
                if (ctrl.currentAbility != null)
                {
                    // Deduct ultimate energy if this was an ultimate
                    bool isUltimate = (ctrl.ctx.abilities.IndexOf(ctrl.currentAbility) == ctrl.ctx.abilities.Count - 1);
                    if (isUltimate)
                    {
                        ctrl.ctx.ultimateEnergy -= ctrl.ctx.ultimateDef.energyRequirement;
                        ctrl.ctx.ultimateReady = false;
                        ctrl.ctx.ultimateCooldownRemaining = ctrl.ctx.ultimateDef.energyRequirement / ctrl.ctx.ultimateDef.cooldownConstant;
                    }
                    // Here we would emit an ability cast event
                }
            }
            public void Exit() { }
            public void Tick(float dt)
            {
                // Immediately transition to cooldown after execution
                ctrl.fsm.Change(ctrl.cooldown);
            }
        }
        private class CooldownState : IAbilityState
        {
            private readonly AbilityController ctrl;
            public CooldownState(AbilityController c) { ctrl = c; }
            public void Enter() { }
            public void Exit() { }
            public void Tick(float dt)
            {
                ctrl.timer -= dt;
                if (ctrl.timer <= 0f)
                {
                    ctrl.fsm.Change(ctrl.idle);
                }
            }
        }
        #endregion
    }
}