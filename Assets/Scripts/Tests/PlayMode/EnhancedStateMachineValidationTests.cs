using NUnit.Framework;
using UnityEngine;
using MOBA.Core;
using MOBA.Controllers;
using MOBA.Data;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for enhanced state machine validation and transition rules.
    /// </summary>
    public class EnhancedStateMachineValidationTests
    {
        private class TestState : IState
        {
            public bool enterCalled = false;
            public bool exitCalled = false;
            public float lastTickDt = 0f;
            
            public void Enter() { enterCalled = true; }
            public void Exit() { exitCalled = true; }
            public void Tick(float dt) { lastTickDt = dt; }
        }
        
        private class ValidState : IState
        {
            public void Enter() { }
            public void Exit() { }  
            public void Tick(float dt) { }
        }
        
        private class InvalidState : IState
        {
            public void Enter() { }
            public void Exit() { }
            public void Tick(float dt) { }
        }

        [Test]
        public void StateMachineRejectsInvalidTransitions()
        {
            var fsm = new StateMachine("TestFSM", "player1");
            var validState = new ValidState();
            var invalidState = new InvalidState();
            
            // Add a rule that rejects transitions to InvalidState
            fsm.AddTransitionRule<ValidState, InvalidState>((from, to, reason) => false);
            
            // Start in valid state
            Assert.IsTrue(fsm.Change(validState, "Initialize"));
            Assert.AreEqual(validState, fsm.Current);
            
            // Attempt invalid transition
            Assert.IsFalse(fsm.Change(invalidState, "Should fail"));
            Assert.AreEqual(validState, fsm.Current); // Should stay in original state
            Assert.IsNotNull(fsm.LastValidationError);
        }

        [Test]
        public void StateMachineAcceptsValidTransitions()
        {
            var fsm = new StateMachine("TestFSM", "player1");
            var state1 = new TestState();
            var state2 = new TestState();
            
            // Add a rule that requires a specific reason
            fsm.AddTransitionRule<TestState, TestState>((from, to, reason) => 
                reason != null && reason.Contains("Valid"));
            
            // Start in first state
            fsm.Change(state1, "Initialize");
            Assert.AreEqual(state1, fsm.Current);
            
            // Valid transition with proper reason
            Assert.IsTrue(fsm.Change(state2, "Valid transition"));
            Assert.AreEqual(state2, fsm.Current);
            Assert.IsTrue(state1.exitCalled);
            Assert.IsTrue(state2.enterCalled);
        }

        [Test]
        public void StateMachineRequiresReasonForCriticalStates()
        {
            var fsm = new StateMachine("TestFSM", "player1");
            var normalState = new ValidState();
            var errorState = new ErrorState();
            
            // Transition to error state without reason should fail
            fsm.Change(normalState, "Start");
            Assert.IsFalse(fsm.Change(errorState)); // No reason provided
            Assert.AreEqual(normalState, fsm.Current);
            
            // With reason should succeed
            Assert.IsTrue(fsm.Change(errorState, "Critical failure"));
            Assert.AreEqual(errorState, fsm.Current);
        }

        [Test]
        public void StateMachineTracksFailedTransitions()
        {
            var fsm = new StateMachine("TestFSM", "player1");
            var state1 = new ValidState();
            var state2 = new InvalidState();
            
            // Add rule that always fails
            fsm.AddTransitionRule<ValidState, InvalidState>((from, to, reason) => false);
            
            fsm.Change(state1, "Start");
            Assert.AreEqual(0, fsm.FailedTransitions);
            
            // Attempt invalid transition multiple times
            fsm.Change(state2, "Fail 1");
            fsm.Change(state2, "Fail 2");
            fsm.Change(state2, "Fail 3");
            
            Assert.AreEqual(3, fsm.FailedTransitions);
        }

        [Test]
        public void StateMachineHandlesExceptionsInStateMethods()
        {
            var fsm = new StateMachine("TestFSM", "player1");
            var faultyState = new FaultyState();
            
            // Enter should not throw despite state throwing exception
            Assert.IsTrue(fsm.Change(faultyState, "Test exception handling"));
            
            // Update should handle exceptions gracefully
            Assert.DoesNotThrow(() => fsm.Update(0.016f));
        }

        [Test]
        public void StateMachineTelemetryProvideUsefulData()
        {
            var fsm = new StateMachine("TestFSM", "player123");
            var state1 = new ValidState();
            var state2 = new InvalidState();
            
            fsm.AddTransitionRule<ValidState, InvalidState>((from, to, reason) => false);
            fsm.Change(state1, "Initialize");
            fsm.Change(state2, "This should fail");
            
            var telemetry = fsm.GetTelemetry();
            
            Assert.AreEqual("TestFSM", telemetry.Name);
            Assert.AreEqual("player123", telemetry.PlayerId);
            Assert.AreEqual("ValidState", telemetry.CurrentState);
            Assert.AreEqual("Initialize", telemetry.LastTransitionReason);
            Assert.AreEqual(1, telemetry.FailedTransitions);
            Assert.IsNotNull(telemetry.LastValidationError);
        }

        private class ErrorState : IState
        {
            public void Enter() { }
            public void Exit() { }
            public void Tick(float dt) { }
        }

        private class FaultyState : IState
        {
            public void Enter() { throw new System.Exception("Enter failed"); }
            public void Exit() { throw new System.Exception("Exit failed"); }
            public void Tick(float dt) { throw new System.Exception("Tick failed"); }
        }
    }
}
