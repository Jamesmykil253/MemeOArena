using NUnit.Framework;
using UnityEngine;
using MOBA.Networking;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for networking message serialization and basic functionality
    /// </summary>
    public class NetworkingTests
    {
        [Test]
        public void InputCmdSerializationWorks()
        {
            var input = new InputCmd(123, new Vector2(0.5f, -0.3f), true, false, true, false, true);
            
            Assert.AreEqual(123u, input.sequenceNumber);
            Assert.AreEqual(0.5f, input.moveInput.x, 0.01f);
            Assert.AreEqual(-0.3f, input.moveInput.y, 0.01f);
            Assert.IsTrue(input.jumpPressed);
            Assert.IsFalse(input.ability1Pressed);
            Assert.IsTrue(input.ability2Pressed);
            Assert.IsFalse(input.ultimatePressed);
            Assert.IsTrue(input.scoringPressed);
        }
        
        [Test]
        public void SnapshotSerializationWorks()
        {
            var snapshot = new Snapshot(
                42u, // lastProcessedSeq
                100u, // tick
                new Vector3(1f, 2f, 3f), // position
                new Vector3(0.5f, 0f, 0.5f), // velocity
                75f, // ultimate energy
                5, // carried points
                80, // current HP
                1, // locomotion state
                2, // ability state
                0  // scoring state
            );
            
            Assert.AreEqual(42u, snapshot.lastProcessedSeq);
            Assert.AreEqual(100u, snapshot.tick);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), snapshot.position);
            Assert.AreEqual(5, snapshot.carriedPoints);
            Assert.AreEqual(80, snapshot.currentHP);
        }
        
        [Test]
        public void GameEventSerializationWorks()
        {
            var gameEvent = new GameEvent(
                GameEvent.EventType.ScoreDeposit,
                150u, // tick
                "player1",
                "target1",
                25, // value
                new Vector3(10f, 0f, 5f) // position
            );
            
            Assert.AreEqual(GameEvent.EventType.ScoreDeposit, gameEvent.eventType);
            Assert.AreEqual(150u, gameEvent.tick);
            Assert.AreEqual("player1", gameEvent.playerId);
            Assert.AreEqual("target1", gameEvent.targetId);
            Assert.AreEqual(25, gameEvent.value);
            Assert.AreEqual(new Vector3(10f, 0f, 5f), gameEvent.position);
        }
        
        [Test]
        public void GameSnapshotCopyConstructorWorks()
        {
            var original = new GameSnapshot();
            original.position = new Vector3(5f, 1f, 3f);
            original.velocity = new Vector3(2f, 0f, 1f);
            original.ultimateEnergy = 50f;
            original.carriedPoints = 3;
            original.currentHP = 90;
            original.tick = 200u;
            
            var copy = new GameSnapshot(original);
            
            Assert.AreEqual(original.position, copy.position);
            Assert.AreEqual(original.velocity, copy.velocity);
            Assert.AreEqual(original.ultimateEnergy, copy.ultimateEnergy);
            Assert.AreEqual(original.carriedPoints, copy.carriedPoints);
            Assert.AreEqual(original.currentHP, copy.currentHP);
            Assert.AreEqual(original.tick, copy.tick);
        }
    }
}
