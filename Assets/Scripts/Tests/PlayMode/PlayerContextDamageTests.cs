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
            baseStats.MaxHP = 100;
            var ctx = new PlayerContext("player", baseStats, null, null);
            ctx.currentHP = 100;
            ctx.ApplyDamage(30);
            Assert.AreEqual(70, ctx.currentHP);
        }

        [Test]
        public void ApplyDamageClampsAtZero()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MaxHP = 20;
            var ctx = new PlayerContext("player", baseStats, null, null);
            ctx.currentHP = 20;
            ctx.ApplyDamage(50);
            Assert.AreEqual(0, ctx.currentHP);
        }

        [Test]
        public void TakeDamageCallsApplyDamage()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MaxHP = 100;
            var ctx = new PlayerContext("player", baseStats, null, null);
            ctx.currentHP = 100;
            ctx.ApplyDamage(25);
            Assert.AreEqual(75, ctx.currentHP);
        }
    }
}
