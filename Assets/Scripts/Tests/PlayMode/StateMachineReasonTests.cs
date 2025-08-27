using NUnit.Framework;
using MOBA.Core;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests that StateMachine records transition reasons correctly.
    /// </summary>
    public class StateMachineReasonTests
    {
        private class DummyState : IState
        {
            public void Enter() { }
            public void Exit() { }
            public void Tick(float dt) { }
        }

        [Test]
        public void ChangeWithReasonSetsReason()
        {
            var sm = new StateMachine();
            var s1 = new DummyState();
            var s2 = new DummyState();
            sm.Change(s1, "Initial");
            sm.Change(s2, "SpawnComplete");
            Assert.AreEqual("SpawnComplete", sm.LastTransitionReason);
        }

        [Test]
        public void ChangeWithoutReasonLeavesReasonNull()
        {
            var sm = new StateMachine();
            var s1 = new DummyState();
            var s2 = new DummyState();
            sm.Change(s1);
            sm.Change(s2);
            Assert.IsNull(sm.LastTransitionReason);
        }
    }
}
