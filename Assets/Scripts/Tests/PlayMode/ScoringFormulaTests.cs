using NUnit.Framework;
using UnityEngine;
using MOBA.Data;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for scoring formula calculations in ScoringDef.
    /// </summary>
    public class ScoringFormulaTests
    {
        private ScoringDef CreateDef()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.thresholds = new[] { 6, 12, 18 };
            def.baseTimes = new[] { 0.5f, 1.0f, 1.5f };
            def.allySynergyMultipliers = new[] { 1.0f, 0.8f, 0.6f };
            def.additiveFactors.Clear();
            def.additiveFactors.Add(new ScoringDef.SpeedFactor { id = "buff1", value = 0.2f });
            def.additiveFactors.Add(new ScoringDef.SpeedFactor { id = "buff2", value = 0.3f });
            return def;
        }

        [Test]
        public void BaseTimeSelection()
        {
            var def = CreateDef();
            Assert.AreEqual(0.5f, def.GetBaseTime(5));
            Assert.AreEqual(0.5f, def.GetBaseTime(6));
            Assert.AreEqual(1.0f, def.GetBaseTime(12));
            Assert.AreEqual(1.5f, def.GetBaseTime(20));
        }

        [Test]
        public void SumSpeedFactorsAddsCorrectly()
        {
            var def = CreateDef();
            Assert.AreEqual(0.2f, def.SumSpeedFactors(new[] { "buff1" }));
            Assert.AreEqual(0.5f, def.SumSpeedFactors(new[] { "buff1", "buff2" }));
        }

        [Test]
        public void SynergyMultiplierClampsAtLastValue()
        {
            var def = CreateDef();
            Assert.AreEqual(1.0f, def.GetSynergyMultiplier(0));
            Assert.AreEqual(0.8f, def.GetSynergyMultiplier(1));
            Assert.AreEqual(0.6f, def.GetSynergyMultiplier(5)); // capped at last value
        }
    }
}
