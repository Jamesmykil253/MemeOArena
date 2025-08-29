using NUnit.Framework;
using UnityEngine;
using MOBA.Data;
using MOBA.Controllers;

namespace Tests.Editor
{
    /// <summary>
    /// Tests for ultimate energy regeneration and gating. Uses the simplified AbilityController.
    /// </summary>
    public class UltimateEnergyTests
    {
        [Test]
        public void UltimateBecomesAvailableWhenEnergyFull()
        {
            var energyDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            energyDef.maxEnergy = 100f;
            energyDef.regenRate = 10f;
            energyDef.energyRequirement = 90f;
            
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            var ctx = new PlayerContext("test", baseStats, energyDef, scoringDef);
            ctx.ultimateEnergy = 0f;
            
            var ability = new AbilityController(ctx);
            
            // Simulate 9 seconds of regen at 10 per second → 90 energy
            for (int i = 0; i < 90; i++) ability.Update(0.1f);
            Assert.IsTrue(ability.IsUltimateReady);
        }
    }
}