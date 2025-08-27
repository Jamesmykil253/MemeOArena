using UnityEngine;
using MOBA.Data;

namespace MOBA.Energy
{
    /// <summary>
    /// Handles accumulation and consumption of ultimate energy.  Each player
    /// context holds its own energy pool.  This system increments energy on
    /// events and enforces cooldown after ultimate use.  It is intentionally
    /// lightweight; ability controllers may bypass this system if they manage
    /// energy themselves.
    /// </summary>
    public class UltimateEnergySystem
    {
        private readonly PlayerContext ctx;
        public UltimateEnergySystem(PlayerContext context)
        {
            ctx = context;
        }

        /// <summary>
        /// Add energy from any source.  Applies passive regen, combat events,
        /// scoring deposits or comeback.  If the cooldown is active, energy
        /// gains may be suppressed until the cooldown expires.
        /// </summary>
        public void AddEnergy(float amount)
        {
            if (ctx.ultimateCooldownRemaining > 0f)
                return;
            ctx.ultimateEnergy += amount;
            if (ctx.ultimateEnergy >= ctx.ultimateDef.energyRequirement)
            {
                ctx.ultimateReady = true;
            }
        }

        /// <summary>
        /// Should be called each tick to reduce the remaining cooldown.  When
        /// cooldown reaches zero, the player can start accumulating energy
        /// again.
        /// </summary>
        public void Update(float dt)
        {
            if (ctx.ultimateCooldownRemaining > 0f)
            {
                ctx.ultimateCooldownRemaining -= dt;
                if (ctx.ultimateCooldownRemaining <= 0f)
                {
                    ctx.ultimateCooldownRemaining = 0f;
                }
            }
        }

        /// <summary>
        /// Consume energy to cast an ultimate and start the cooldown.  Returns
        /// true if the ultimate was successfully consumed.
        /// </summary>
        public bool ConsumeUltimate()
        {
            if (!ctx.ultimateReady) return false;
            ctx.ultimateEnergy -= ctx.ultimateDef.energyRequirement;
            ctx.ultimateReady = false;
            ctx.ultimateCooldownRemaining = ctx.ultimateDef.energyRequirement / ctx.ultimateDef.cooldownConstant;
            if (ctx.ultimateEnergy < 0f)
                ctx.ultimateEnergy = 0f;
            return true;
        }
    }
}