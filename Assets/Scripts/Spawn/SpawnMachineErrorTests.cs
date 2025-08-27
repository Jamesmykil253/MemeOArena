using NUnit.Framework;
using MOBA.Data;
using MOBA.Spawn;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests that SpawnMachine sets LastError correctly on error events.
    /// </summary>
    public class SpawnMachineErrorTests
    {
        private SpawnMachine CreateMachine()
        {
            var baseStats = UnityEngine.ScriptableObject.CreateInstance<BaseStatsTemplate>();
            var ctx = new PlayerContext("player", baseStats, null, null);
            return new SpawnMachine(ctx);
        }

        [Test]
        public void SetupErrorSetsLastError()
        {
            var machine = CreateMachine();
            machine.SetupError();
            Assert.AreEqual(SpawnMachine.SpawnError.SetupFailed, machine.LastError);
        }

        [Test]
        public void AssignmentErrorSetsLastError()
        {
            var machine = CreateMachine();
            machine.AssignmentError();
            Assert.AreEqual(SpawnMachine.SpawnError.AssignmentFailed, machine.LastError);
        }

        [Test]
        public void ValidationFailureSetsLastError()
        {
            var machine = CreateMachine();
            machine.ValidationFailure();
            Assert.AreEqual(SpawnMachine.SpawnError.ValidationFailed, machine.LastError);
        }

        [Test]
        public void FinalizationErrorSetsLastError()
        {
            var machine = CreateMachine();
            machine.FinalizationError();
            Assert.AreEqual(SpawnMachine.SpawnError.FinalizationFailed, machine.LastError);
        }
    }
}
