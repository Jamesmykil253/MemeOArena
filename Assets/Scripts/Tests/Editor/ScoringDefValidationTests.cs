using NUnit.Framework;
using System;
using UnityEngine;
using MOBA.Data;

namespace Tests.Editor
{
    /// <summary>
    /// Tests for the ScoringDef public methods and behavior.
    /// </summary>
    public class ScoringDefValidationTests
    {
        [Test]
        public void GetBaseTimeReturnsCorrectValues()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.baseTimes = new float[] { 1f, 2f, 3f };
            
            Assert.AreEqual(1f, def.GetBaseTime(0));
            Assert.AreEqual(2f, def.GetBaseTime(1));
            Assert.AreEqual(3f, def.GetBaseTime(2));
        }

        [Test]
        public void GetBaseTimeHandlesInvalidIndices()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.baseTimes = new float[] { 1f, 2f };
            
            Assert.AreEqual(0f, def.GetBaseTime(-1));
            Assert.AreEqual(0f, def.GetBaseTime(5));
        }

        [Test]
        public void SumSpeedFactorsCalculatesCorrectly()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.additiveSpeedFactors = new float[] { 0.1f, 0.2f, 0.3f };
            
            Assert.AreEqual(0.6f, def.SumSpeedFactors(), 0.001f);
        }

        [Test]
        public void GetSynergyMultiplierReturnsCorrectValues()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.synergyMultipliers = new float[] { 1f, 0.9f, 0.8f };
            
            Assert.AreEqual(1f, def.GetSynergyMultiplier(0));
            Assert.AreEqual(0.9f, def.GetSynergyMultiplier(1));
            Assert.AreEqual(0.8f, def.GetSynergyMultiplier(2));
        }
    }
}