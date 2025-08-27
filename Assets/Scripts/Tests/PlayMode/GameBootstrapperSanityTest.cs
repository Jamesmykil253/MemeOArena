using NUnit.Framework;
using UnityEngine;
using MOBA.Bootstrap;
using MOBA.Data;
using MOBA.Controllers;
using MOBA.Spawn;

namespace Tests.PlayMode
{
    /// <summary>
    /// Sanity test ensuring the GameBootstrapper initializes without errors.
    /// </summary>
    public class GameBootstrapperSanityTest
    {
        [Test]
        public void BootstrapperInitializesControllers()
        {
            // Arrange: create fake ScriptableObjects
            var baseStats = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            baseStats.MaxHP = 100f;
            baseStats.Attack = 10f;
            baseStats.Defense = 5f;
            baseStats.MoveSpeed = 5f;

            var ultDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            var scoreDef = ScriptableObject.CreateInstance<ScoringDef>();
            var ability = ScriptableObject.CreateInstance<AbilityDef>();
            var inputActions = new InputSystem_Actions();

            var go = new GameObject();
            var bootstrap = go.AddComponent<GameBootstrapper>();
            bootstrap.baseStats = baseStats;
            bootstrap.ultimateDef = ultDef;
            bootstrap.scoringDef = scoreDef;
            bootstrap.abilities = new[] { ability };
            bootstrap.inputActions = inputActions;

            // Act
            bootstrap.Initialize();

            // Use reflection to assert private fields are set
            var locomotionField = typeof(GameBootstrapper).GetField("locomotion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var locomotion = (LocomotionController)locomotionField.GetValue(bootstrap);
            Assert.NotNull(locomotion);
        }
    }
}
