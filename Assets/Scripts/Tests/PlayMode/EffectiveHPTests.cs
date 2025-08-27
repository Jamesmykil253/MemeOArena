using NUnit.Framework;
using MOBA.Combat;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for the effective HP calculation.
    /// </summary>
    public class EffectiveHPTests
    {
        [Test]
        public void EffectiveHPCalculatesCorrectly()
        {
            float maxHP = 10000f;
            float defense = 600f;
            float expected = maxHP * (1f + defense / 600f); // 10000 * (1 + 1) = 20000
            float actual = CombatSystem.ComputeEffectiveHP(maxHP, defense);
            Assert.AreEqual(expected, actual, 0.001f);
        }
    }
}
