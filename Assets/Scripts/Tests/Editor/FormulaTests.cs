using NUnit.Framework;
using System;

namespace Tests.Editor
{
    /// <summary>
    /// Unit tests for core gameplay formulas. These tests replicate the authoritative formulas defined in the docs without depending on gameplay code.
    /// </summary>
    public class FormulaTests
    {
        [Test]
        public void EffectiveHP_CalculatesCorrectly()
        {
            // Example from EffectiveHP.md: MaxHP 10_000 with 200 Defense yields 13_333 effective HP.
            int maxHP = 10000;
            int defense = 200;
            double eHP = maxHP * (1 + defense / 600.0);
            Assert.AreEqual(13333.333, eHP, 0.001);
        }

        [Test]
        public void RawDamage_RSBFormula()
        {
            // RSB coefficients and inputs
            double R = 1.2;
            double S = 3.0;
            int B = 10;
            int attack = 100;
            int level = 5;
            // Formula: rawDamage = floor(R * Attack + S * (Level – 1) + B)
            int rawDamage = (int)Math.Floor(R * attack + S * (level - 1) + B);
            Assert.AreEqual(142, rawDamage);
        }

        [Test]
        public void DamageTaken_DefenseMitigation()
        {
            // Given rawDamage 100 and Defense 200, damage is mitigated by 600/(600+Defense)
            int rawDamage = 100;
            int defense = 200;
            int damage = (int)Math.Floor(rawDamage * 600.0 / (600.0 + defense));
            Assert.AreEqual(75, damage);
        }

        [Test]
        public void ChannelTime_ScoringFormula()
        {
            // Base time for depositing 20 points is 2 seconds (example from ScoringFormulas.md)
            double baseTime = 2.0;
            // Goal Getter adds +1 speed factor → speedMultiplier = 1 + 1
            double speedMultiplier = 1 + 1.0;
            // One ally on pad reduces time by 30 % (multiplier 0.70)
            double teamSynergy = 0.70;
            // Final channel time formula: (baseTime / speedMultiplier) * teamSynergy
            double channelTime = (baseTime / speedMultiplier) * teamSynergy;
            Assert.AreEqual(0.70, channelTime, 0.001);
        }
    }
}