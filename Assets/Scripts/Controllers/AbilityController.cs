using UnityEngine;
using MOBA.Data;

namespace MOBA.Controllers
{
    /// <summary>
    /// Simplified ability controller that regenerates ultimate energy over time and reports readiness.
    /// </summary>
    public class AbilityController
    {
        private readonly PlayerContext context;
        public AbilityController(PlayerContext ctx) { context = ctx; }
        public bool IsUltimateReady => context.ultimateEnergy >= context.ultimateDef.energyRequirement;

        /// <summary>
        /// Update ultimate energy by regenRate * dt, capped at maxEnergy.
        /// </summary>
        public void Update(float dt)
        {
            if (context.ultimateDef == null) return;
            float newEnergy = context.ultimateEnergy + context.ultimateDef.regenRate * dt;
            context.ultimateEnergy = Mathf.Min(context.ultimateDef.maxEnergy, newEnergy);
        }
    }
}