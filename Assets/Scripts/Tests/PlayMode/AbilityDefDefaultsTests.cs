using NUnit.Framework;
using MOBA.Data;
using UnityEngine;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests default values for newly created AbilityDef instances.
    /// </summary>
    public class AbilityDefDefaultsTests
    {
        [Test]
        public void DefaultCriticalAndPenetrationValues()
        {
            var def = ScriptableObject.CreateInstance<AbilityDef>();
            Assert.AreEqual(0f, def.CritChance);
            Assert.AreEqual(1.5f, def.CritMultiplier);
            Assert.AreEqual(0f, def.DefensePenetration);
        }
    }
}
