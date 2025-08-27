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
            def.synergyMultipliers = new[] { 1.0f, 0.8f, 0.6f };
            def.additiveSpeedFactors = new[] { 0.2f, 0.3f, 0.1f };
            return def;
        }

        [Test]
        public void BaseTimeSelection()
        {
            var def = CreateDef();
            Assert.AreEqual(0.5f, def.GetBaseTime(0));
            Assert.AreEqual(1.0f, def.GetBaseTime(1));
            Assert.AreEqual(1.5f, def.GetBaseTime(2));
        }

        [Test]
        public void SumSpeedFactorsAddsCorrectly()
        {
            var def = CreateDef();
            // Test the sum of all speed factors
            Assert.AreEqual(0.6f, def.SumSpeedFactors(), 0.001f);
        }

        [Test]
        public void SynergyMultiplierReturnsCorrectValues()
        {
            var def = CreateDef();
            Assert.AreEqual(1.0f, def.GetSynergyMultiplier(0));
            Assert.AreEqual(0.8f, def.GetSynergyMultiplier(1));
            Assert.AreEqual(0.6f, def.GetSynergyMultiplier(2));
            Assert.AreEqual(1.0f, def.GetSynergyMultiplier(5)); // returns 1.0f for invalid indices
        }
    }
}
