using NUnit.Framework;
using UnityEngine;
using MOBA.Data;
using MOBA.Controllers;
using MOBA.Combat;
using System.Collections.Generic;

namespace Tests.Editor
{
    public class AbilityControllerTests
    {
        [Test]
        public void AbilityController_RegeneratesEnergyOverTime()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.Attack = 50;
            
            var ultimateDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            ultimateDef.maxEnergy = 100f;
            ultimateDef.regenRate = 10f;
            ultimateDef.energyRequirement = 80f;
            
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            var ctx = new PlayerContext("test", baseStats, ultimateDef, scoringDef);
            ctx.ultimateEnergy = 0f;

            var controller = new AbilityController(ctx);
            
            // Should not be ready initially
            Assert.IsFalse(controller.IsUltimateReady);
            
            // Simulate 8 seconds of regen at 10 per second → 80 energy
            for (int i = 0; i < 80; i++)
            {
                controller.Update(0.1f);
            }
            
            Assert.IsTrue(controller.IsUltimateReady);
            Assert.AreEqual(80f, ctx.ultimateEnergy, 0.1f);
        }

        [Test]
        public void AbilityController_CapsEnergyAtMaximum()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            var ultimateDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            ultimateDef.maxEnergy = 50f;
            ultimateDef.regenRate = 10f;
            ultimateDef.energyRequirement = 40f;
            
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            var ctx = new PlayerContext("test", baseStats, ultimateDef, scoringDef);
            ctx.ultimateEnergy = 45f; // Start near cap

            var controller = new AbilityController(ctx);
            
            // Update beyond the cap
            controller.Update(1.0f); // Would add 10, but should cap at 50
            
            Assert.AreEqual(50f, ctx.ultimateEnergy);
        }
    }
}
