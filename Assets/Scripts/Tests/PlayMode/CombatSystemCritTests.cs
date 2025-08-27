using NUnit.Framework;
using MOBA.Combat;
using MOBA.Data;
using UnityEngine;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for CombatSystem.ComputeFinalDamage including crits and penetration.
    /// </summary>
    public class CombatSystemCritTests
    {
        private AbilityDef CreateAbility(float ratio, float slider, float baseValue,
                                         float critChance, float critMultiplier, float penetration)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDef>();
            ability.Ratio = ratio;
            ability.Slider = slider;
            ability.Base = baseValue;
            ability.CritChance = critChance;
            ability.CritMultiplier = critMultiplier;
            ability.DefensePenetration = penetration;
            return ability;
        }

        [Test]
        public void NoCritWhenChanceZero()
        {
            var ability = CreateAbility(1f, 0f, 0f, 0f, 2f, 0f);
            var rng = new System.Random(42);
            int dmg = CombatSystem.ComputeFinalDamage(100f, 1, ability, 0f, rng);
            // Expected raw = 100, mitigated = 100 (no defense), no crit
            Assert.AreEqual(100, dmg);
        }

        [Test]
        public void AlwaysCritWhenChanceOne()
        {
            var ability = CreateAbility(1f, 0f, 0f, 1f, 2f, 0f);
            var rng = new System.Random(42);
            int dmg = CombatSystem.ComputeFinalDamage(100f, 1, ability, 0f, rng);
            // Base damage 100 * crit multiplier 2
            Assert.AreEqual(200, dmg);
        }

        [Test]
        public void DefensePenetrationIncreasesDamage()
        {
            var abilityNoPen = CreateAbility(1f, 0f, 0f, 0f, 2f, 0f);
            var abilityPen = CreateAbility(1f, 0f, 0f, 0f, 2f, 50f);
            var rng = new System.Random(1);
            int dmgNoPen = CombatSystem.ComputeFinalDamage(100f, 1, abilityNoPen, 100f, rng);
            int dmgPen = CombatSystem.ComputeFinalDamage(100f, 1, abilityPen, 100f, rng);
            Assert.Greater(dmgPen, dmgNoPen);
        }
    }
}
