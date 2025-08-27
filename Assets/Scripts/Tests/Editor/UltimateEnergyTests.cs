using NUnit.Framework;

/// <summary>
/// Tests for ultimate energy regeneration and gating. Uses the simplified AbilityController.
/// </summary>
public class UltimateEnergyTests
{
    [Test]
    public void UltimateBecomesAvailableWhenEnergyFull()
    {
        var energyDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
        energyDef.maxEnergy = 100f;
        energyDef.regenRate = 10f;
        energyDef.required = 90f;
        var ctx = new PlayerContext { ultimateEnergy = 0f, ultimateDef = energyDef };
        var ability = new AbilityController(ctx);
        // Simulate 9 seconds of regen at 10 per second → 90 energy
        for (int i = 0; i < 90; i++) ability.Update(0.1f);
        Assert.IsTrue(ability.IsUltimateReady);
    }
}