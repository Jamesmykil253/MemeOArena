using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Controllers
{
    /// <summary>
    /// Handles the collection and deposit of points using a finite state machine.
    /// When the player carries points they can begin channeling on a scoring
    /// pad.  Channeling time depends on the number of points, active speed
    /// buffs and ally synergy.  Channeling is cancelled on movement or
    /// damage.  On successful deposit, energy is awarded and points reset.
    /// </summary>
    public class ScoringController
    {
        private readonly PlayerContext ctx;
        private readonly StateMachine fsm = new StateMachine();
        private readonly CarryingState carrying;
        private readonly ChannelingState channeling;
        private readonly DepositedState deposited;
        private readonly InterruptedState interrupted;

        // Channeling progress
        private float channelTimer;
        private float totalChannelTime;
        private int alliesPresent;

        public ScoringController(PlayerContext context)
        {
            ctx = context;
            carrying = new CarryingState(this);
            channeling = new ChannelingState(this);
            deposited = new DepositedState(this);
            interrupted = new InterruptedState(this);
            fsm.Change(carrying);
        }

        public void Update(float dt)
        {
            fsm.Update(dt);
        }

        /// <summary>
        /// Called when the player picks up points.  Adds to carriedPoints.
        /// </summary>
        public void AddPoints(int amount)
        {
            ctx.carriedPoints += amount;
        }

        /// <summary>
        /// Begin channeling if carrying any points.  allies is the number of
        /// allies (excluding the carrier) standing on the scoring pad.
        /// activeBuffs contains identifiers of active scoring speed buffs.
        /// </summary>
        public void StartChanneling(int allies, IEnumerable<string> activeBuffs)
        {
            if (ctx.carriedPoints <= 0)
                return;
            alliesPresent = allies;
            // Compute total channel time
            float baseTime = ctx.scoringDef.GetBaseTime(ctx.carriedPoints);
            float additive = ctx.scoringDef.SumSpeedFactors(activeBuffs);
            float speedMult = 1f + additive;
            totalChannelTime = baseTime / speedMult * ctx.scoringDef.GetSynergyMultiplier(allies);
            channelTimer = totalChannelTime;
            fsm.Change(channeling);
        }

        /// <summary>
        /// Interrupt channeling (e.g. on movement or damage).  Drops all points
        /// to the ground and resets the FSM to Carrying with zero points.
        /// </summary>
        public void Interrupt()
        {
            if (fsm.Current == channeling)
            {
                fsm.Change(interrupted);
            }
        }

        #region States
        private class CarryingState : IState
        {
            private readonly ScoringController ctrl;
            public CarryingState(ScoringController c) { ctrl = c; }
            public void Enter() { }
            public void Exit() { }
            public void Tick(float dt)
            {
                // Do nothing; waiting for channeling command or more points.
            }
        }
        private class ChannelingState : IState
        {
            private readonly ScoringController ctrl;
            public ChannelingState(ScoringController c) { ctrl = c; }
            public void Enter()
            {
                // Visual feedback could be displayed here
            }
            public void Exit() { }
            public void Tick(float dt)
            {
                ctrl.channelTimer -= dt;
                // In a full implementation movement or damage would call Interrupt()
                if (ctrl.channelTimer <= 0f)
                {
                    ctrl.fsm.Change(ctrl.deposited);
                }
            }
        }
        private class DepositedState : IState
        {
            private readonly ScoringController ctrl;
            public DepositedState(ScoringController c) { ctrl = c; }
            public void Enter()
            {
                // Award points and energy
                // In a real implementation this would update the scoreboard
                // and grant energy via the UltimateEnergySystem.
                ctrl.ctx.ultimateEnergy += ctrl.ctx.ultimateDef.scoreDepositEnergy;
                if (ctrl.ctx.ultimateEnergy >= ctrl.ctx.ultimateDef.energyRequirement)
                {
                    ctrl.ctx.ultimateReady = true;
                }
                ctrl.ctx.carriedPoints = 0;
                // Return to carrying state with zero points
                ctrl.fsm.Change(ctrl.carrying);
            }
            public void Exit() { }
            public void Tick(float dt) { }
        }
        private class InterruptedState : IState
        {
            private readonly ScoringController ctrl;
            public InterruptedState(ScoringController c) { ctrl = c; }
            public void Enter()
            {
                // Drop orbs on the ground: in a full implementation, spawn
                // pickups at the player's location.
                ctrl.ctx.carriedPoints = 0;
                ctrl.fsm.Change(ctrl.carrying);
            }
            public void Exit() { }
            public void Tick(float dt) { }
        }
        #endregion
    }
}