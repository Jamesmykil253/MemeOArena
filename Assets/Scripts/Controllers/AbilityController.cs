using MOBA.Core;
using MOBA.Data;
using MOBA.Actors;
using System.Collections.Generic;
using UnityEngine;
using MOBA.Combat;
using MOBA.Controllers;

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
        private readonly IAbilityTargetProvider targetProvider;

        public AbilityController(PlayerContext context, IAbilityTargetProvider provider = null)
        {
            ctx = context;
            targetProvider = provider ?? FirstObjectDummyTargetProvider.Instance;
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
                // TODO: cancel on movement/damage flags via LocomotionController hooks.
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
                // Apply the ability’s effect.
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

                    var target = ctrl.targetProvider?.FindPrimaryTarget(ctrl.ctx);
                    if (target != null)
                    {
                        // Cast after multiply to avoid truncation.
                        int damage = (int)(ctrl.currentAbility.Ratio * ctrl.ctx.attack);
                        target.TakeDamage(damage);
                    }
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
