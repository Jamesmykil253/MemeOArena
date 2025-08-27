using NUnit.Framework;
using MOBA.Data;
using UnityEngine;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for AbilityDef damage type default value.
    /// </summary>
    public class AbilityDefDamageTypeTest
    {
        [Test]
        public void DefaultDamageTypeIsPhysical()
        {
            var def = ScriptableObject.CreateInstance<AbilityDef>();
            Assert.AreEqual(AbilityDef.DamageType.Physical, def.damageType);
        }
    }
}
