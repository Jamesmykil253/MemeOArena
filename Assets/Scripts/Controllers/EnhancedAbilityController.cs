using UnityEngine;
using MOBA.Data;
using MOBA.Core;
using MOBA.Combat;

namespace MOBA.Controllers
{
    /// <summary>
    /// Complete ability controller with FSM states: Idle → Casting → Executing → Cooldown → Idle
    /// Handles ability casting, interruption, target acquisition, and cooldown management.
    /// </summary>
    public class EnhancedAbilityController
    {
        private readonly PlayerContext context;
        private readonly IAbilityTargetProvider targetProvider;
        private readonly StateMachine fsm;
        
        // States
        private readonly IdleState idleState;
        private readonly CastingState castingState;
        private readonly ExecutingState executingState;
        private readonly CooldownState cooldownState;
        
        // Current ability being cast
        private AbilityDef currentAbility;
        private IDamageable currentTarget;
        private float stateTimer;
        
        public bool IsIdle => fsm.Current == idleState;
        public bool IsCasting => fsm.Current == castingState;
        public bool IsExecuting => fsm.Current == executingState;
        public bool IsOnCooldown => fsm.Current == cooldownState;
        public bool IsUltimateReady => context.ultimateEnergy >= context.ultimateDef.required;
        public AbilityDef CurrentAbility => currentAbility;
        
        // Events
        public System.Action<AbilityDef> OnAbilityCastStart;
        public System.Action<AbilityDef, IDamageable> OnAbilityExecute;
        public System.Action<AbilityDef, string> OnAbilityInterrupt; // reason
        public System.Action<AbilityDef> OnAbilityCooldownStart;
        
        public EnhancedAbilityController(PlayerContext ctx, IAbilityTargetProvider targetProvider = null)
        {
            context = ctx;
            this.targetProvider = targetProvider ?? FirstObjectDummyTargetProvider.Instance;
            
            // Initialize FSM and states
            fsm = new StateMachine("AbilityFSM", ctx.playerId);
            idleState = new IdleState(this);
            castingState = new CastingState(this);
            executingState = new ExecutingState(this);
            cooldownState = new CooldownState(this);
            
            // Start in idle state
            fsm.Change(idleState, "Initialized");
        }
        
        public void Update(float dt)
        {
            // Update ultimate energy
            UpdateUltimateEnergy(dt);
            
            // Update FSM
            fsm.Update(dt);
        }
        
        private void UpdateUltimateEnergy(float dt)
        {
            if (context.ultimateDef == null) return;
            
            // Update ultimate cooldown
            if (context.ultimateCooldownRemaining > 0f)
            {
                context.ultimateCooldownRemaining -= dt;
                if (context.ultimateCooldownRemaining <= 0f)
                {
                    context.ultimateCooldownRemaining = 0f;
                }
            }
            
            // Regenerate energy if not on cooldown
            if (context.ultimateCooldownRemaining <= 0f)
            {
                float newEnergy = context.ultimateEnergy + context.ultimateDef.regenRate * dt;
                context.ultimateEnergy = Mathf.Min(context.ultimateDef.maxEnergy, newEnergy);
            }
        }
        
        /// <summary>
        /// Attempt to cast an ability. Returns true if cast started successfully.
        /// </summary>
        public bool TryCastAbility(AbilityDef ability)
        {
            if (!IsIdle) return false;
            if (ability == null) return false;
            
            // Check if ultimate is ready
            if (IsUltimateAbility(ability) && !IsUltimateReady)
            {
                return false;
            }
            
            // Find target
            IDamageable target = targetProvider.FindPrimaryTarget(context);
            if (target == null) return false;
            
            // Start casting
            StartCasting(ability, target);
            return true;
        }
        
        /// <summary>
        /// Interrupt current ability due to movement
        /// </summary>
        public void InterruptByMovement()
        {
            if (IsCasting)
            {
                fsm.Change(idleState, "Movement interrupt");
                OnAbilityInterrupt?.Invoke(currentAbility, "movement");
            }
        }
        
        /// <summary>
        /// Interrupt current ability due to damage
        /// </summary>
        public void InterruptByDamage()
        {
            if (IsCasting)
            {
                fsm.Change(idleState, "Damage interrupt");
                OnAbilityInterrupt?.Invoke(currentAbility, "damage");
            }
        }
        
        private void StartCasting(AbilityDef ability, IDamageable target)
        {
            currentAbility = ability;
            currentTarget = target;
            stateTimer = 0f;
            
            fsm.Change(castingState, $"Casting {ability.name}");
            OnAbilityCastStart?.Invoke(ability);
        }
        
        private void ExecuteAbility()
        {
            if (currentAbility == null || currentTarget == null) return;
            
            // Calculate damage using combat system
            int level = context.level;
            float attackStat = context.currentAttack;
            System.Random rng = new System.Random(); // In production, use seeded RNG
            
            int finalDamage = CombatSystem.ComputeFinalDamage(
                attackStat, level, currentAbility, 
                currentTarget.GetDefense(), rng);
            
            // Apply damage
            currentTarget.TakeDamage(finalDamage);
            
            // Handle ultimate energy consumption
            if (IsUltimateAbility(currentAbility))
            {
                context.ultimateEnergy = 0f;
                context.ultimateCooldownRemaining = context.ultimateDef.cooldownConstant;
            }
            
            OnAbilityExecute?.Invoke(currentAbility, currentTarget);
            
            // Move to cooldown
            fsm.Change(cooldownState, "Ability executed");
            OnAbilityCooldownStart?.Invoke(currentAbility);
        }
        
        private bool IsUltimateAbility(AbilityDef ability)
        {
            // Check if this is an ultimate ability by comparing with context
            return context.abilities.Count > 1 && context.abilities[1] == ability;
        }
        
        #region State Classes
        
        private class IdleState : IState
        {
            private readonly EnhancedAbilityController controller;
            
            public IdleState(EnhancedAbilityController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter()
            {
                // Clear current ability
                controller.currentAbility = null;
                controller.currentTarget = null;
                controller.stateTimer = 0f;
            }
            
            public void Exit() { }
            
            public void Tick(float dt)
            {
                // Ready to cast abilities - external systems call TryCastAbility
            }
        }
        
        private class CastingState : IState
        {
            private readonly EnhancedAbilityController controller;
            
            public CastingState(EnhancedAbilityController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter()
            {
                controller.stateTimer = 0f;
            }
            
            public void Exit() { }
            
            public void Tick(float dt)
            {
                controller.stateTimer += dt;
                
                // Check if cast time is complete
                if (controller.stateTimer >= controller.currentAbility.CastTime)
                {
                    controller.fsm.Change(controller.executingState, "Cast time complete");
                }
            }
        }
        
        private class ExecutingState : IState
        {
            private readonly EnhancedAbilityController controller;
            
            public ExecutingState(EnhancedAbilityController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter()
            {
                // Execute the ability immediately when entering this state
                controller.ExecuteAbility();
            }
            
            public void Exit() { }
            
            public void Tick(float dt)
            {
                // Execution is instantaneous, should immediately transition to cooldown
            }
        }
        
        private class CooldownState : IState
        {
            private readonly EnhancedAbilityController controller;
            
            public CooldownState(EnhancedAbilityController ctrl)
            {
                controller = ctrl;
            }
            
            public void Enter()
            {
                controller.stateTimer = 0f;
            }
            
            public void Exit() { }
            
            public void Tick(float dt)
            {
                controller.stateTimer += dt;
                
                // Check if cooldown is complete
                if (controller.stateTimer >= controller.currentAbility.Cooldown)
                {
                    controller.fsm.Change(controller.idleState, "Cooldown complete");
                }
            }
        }
        
        #endregion
    }
}
