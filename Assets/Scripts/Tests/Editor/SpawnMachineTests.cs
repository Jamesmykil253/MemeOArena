using NUnit.Framework;
using System;
using UnityEngine;
using MOBA.Spawn;
using MOBA.Data;

namespace Tests.Editor
{
    /// <summary>
    /// Tests for the simplified SpawnMachine.
    /// </summary>
    public class SpawnMachineTests
    {
        [Test]
        public void HappyPathSpawnsPlayer()
        {
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            var ultimateDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            var ctx = new PlayerContext("test", baseStats, ultimateDef, scoringDef);
            
            var machine = new SpawnMachine(ctx);
            machine.SpawnRequest();
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
            var ultimateDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            var ctx = new PlayerContext("test", null, ultimateDef, scoringDef);
            
            var machine = new SpawnMachine(ctx);
            machine.SpawnRequest();
            
            // Simulate updates until finished
            for (int i = 0; i < 10 && !machine.IsFinished; i++)
            {
                machine.Update(0.02f);
            }
            
            Assert.IsTrue(machine.IsFinished);
            Assert.IsFalse(machine.Spawned);
            Assert.AreEqual("Assignment failed", machine.LastError);
        }
    }
}