using NUnit.Framework;
using System;

using MOBA.Spawn;

/// <summary>
/// Tests for the simplified SpawnMachine.
/// </summary>
public class SpawnMachineTests
{
    [Test]
    public void HappyPathSpawnsPlayer()
    {
        var ctx = new PlayerContext { baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>() };
        var machine = new SpawnMachine(ctx);
        machine.TriggerSpawn();
        // Simulate a few frames. Each update should advance the state machine.
        for (int i = 0; i < 4 && !machine.IsFinished; i++)
        {
            machine.Update(0.02f);
        }
        Assert.IsTrue(machine.Spawned);
    }

    [Test]
    public void ErrorWhenMissingBaseStats()
    {
        var ctx = new PlayerContext { baseStats = null };
        var machine = new SpawnMachine(ctx);
        machine.TriggerSpawn();
        Assert.Throws<InvalidOperationException>(() => {
            while (!machine.IsFinished)
            {
                machine.Update(0.02f);
            }
        });
    }
}