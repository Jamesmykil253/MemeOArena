using NUnit.Framework;
using UnityEngine;
using MOBA.Data;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests validation logic in ScoringDef.GetBaseTime().
    /// </summary>
    public class ScoringDefValidationTests
    {
        [Test]
        public void MismatchedLengthsThrowException()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.thresholds = new[] { 10, 20 };
            def.baseTimes = new[] { 1.0f };
            Assert.Throws<System.InvalidOperationException>(() => def.GetBaseTime(15));
        }

        [Test]
        public void UnsortedThresholdsThrowException()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.thresholds = new[] { 30, 20 };
            def.baseTimes = new[] { 1.0f, 2.0f };
            Assert.Throws<System.InvalidOperationException>(() => def.GetBaseTime(25));
        }

        [Test]
        public void ValidDataReturnsCorrectBaseTime()
        {
            var def = ScriptableObject.CreateInstance<ScoringDef>();
            def.thresholds = new[] { 10, 20 };
            def.baseTimes = new[] { 1.0f, 2.0f };
            // Points below first threshold
            Assert.AreEqual(1.0f, def.GetBaseTime(5));
            // Points between thresholds
            Assert.AreEqual(2.0f, def.GetBaseTime(15));
        }
    }
}
