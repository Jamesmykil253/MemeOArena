using NUnit.Framework;
using UnityEngine;
using MOBA.Data;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests that magic defense is copied from the base stats template.
    /// </summary>
    public class MagicDefenseTests
    {
        [Test]
        public void ResetCopiesMagicDefense()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MagicDefense = 50f;
            var ctx = new PlayerContext("player", baseStats, null, null);
            // Magic defense should be copied in constructor via ResetToTemplate
            Assert.AreEqual(50f, ctx.magicDefense);
            // Changing baseStats after creation should not affect context until reset
            baseStats.MagicDefense = 100f;
            ctx.ResetToTemplate();
            Assert.AreEqual(100f, ctx.magicDefense);
        }
    }
}
