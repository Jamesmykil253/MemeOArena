using NUnit.Framework;
using UnityEngine;
using MOBA.Data;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for PlayerContext damage application methods.
    /// </summary>
    public class PlayerContextDamageTests
    {
        [Test]
        public void ApplyDamageReducesHP()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MaxHP = 100f;
            var ctx = new PlayerContext("player", baseStats, null, null);
            ctx.currentHP = 100f;
            ctx.ApplyDamage(30);
            Assert.AreEqual(70f, ctx.currentHP);
        }

        [Test]
        public void ApplyDamageClampsAtZero()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MaxHP = 20f;
            var ctx = new PlayerContext("player", baseStats, null, null);
            ctx.currentHP = 20f;
            ctx.ApplyDamage(50);
            Assert.AreEqual(0f, ctx.currentHP);
        }

        [Test]
        public void TakeDamageCallsApplyDamage()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MaxHP = 100f;
            var ctx = new PlayerContext("player", baseStats, null, null);
            ctx.currentHP = 100f;
            ctx.ApplyDamage(25);
            Assert.AreEqual(75f, ctx.currentHP);
        }
    }
}
