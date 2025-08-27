using NUnit.Framework;
using UnityEngine;
using MOBA.Data;
using MOBA.Controllers;
using MOBA.Combat;
using System.Collections.Generic;

public class AbilityControllerTests
{
    private class FakeDamageable : MonoBehaviour, IDamageable
    {
        public int DamageTakenTotal { get; private set; }
        public void TakeDamage(int amount) => DamageTakenTotal += amount;
    }

    private class FakeProvider : IAbilityTargetProvider
    {
        private readonly IDamageable target;
        public FakeProvider(IDamageable t) { target = t; }
        public IDamageable FindPrimaryTarget(PlayerContext ctx) => target;
    }

    [Test]
    public void Ability_DealsExpectedDamage_AndReturnsToIdle()
    {
        var ctx = new PlayerContext
        {
            attack = 50f,
            abilities = new List<AbilityDef>
            {
                new AbilityDef { Ratio = 2f, CastTime = 0.1f, Cooldown = 0.2f }
            },
            ultimateDef = new UltimateEnergyDef { energyRequirement = 100f, cooldownConstant = 50f }
        };

        var go = new GameObject("FakeTarget");
        var fake = go.AddComponent<FakeDamageable>();
        var provider = new FakeProvider(fake);
        var ctrl = new AbilityController(ctx, provider);

        ctrl.TryCast(0);
        ctrl.Update(0.1f); // complete cast
        ctrl.Update(0.0f); // execute
        ctrl.Update(0.2f); // finish cooldown

        Assert.AreEqual(100, fake.DamageTakenTotal);
    }
}
