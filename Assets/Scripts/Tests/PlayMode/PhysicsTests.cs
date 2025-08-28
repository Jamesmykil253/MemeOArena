using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode
{
    /// <summary>
    /// Tests for physics body settings and basic physics calculations
    /// </summary>
    public class PhysicsTests
    {
        [Test]
        public void PhysicsBodySettingsHaveDefaultValues()
        {
            var settings = MOBA.Physics.PhysicsBodySettings.Default;
            
            Assert.AreEqual(1f, settings.mass);
            Assert.IsTrue(settings.useGravity);
            Assert.IsFalse(settings.isKinematic);
            Assert.AreEqual(0f, settings.drag);
            Assert.AreEqual(0f, settings.angularDrag);
        }
        
        [Test]
        public void PhysicsBodyInitializationWorks()
        {
            var body = new MOBA.Physics.PhysicsBody();
            body.id = "test_body";
            body.position = new Vector3(1f, 2f, 3f);
            body.velocity = new Vector3(0.5f, 0f, 0.2f);
            body.isGrounded = true;
            body.isInKnockback = false;
            
            Assert.AreEqual("test_body", body.id);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), body.position);
            Assert.AreEqual(new Vector3(0.5f, 0f, 0.2f), body.velocity);
            Assert.IsTrue(body.isGrounded);
            Assert.IsFalse(body.isInKnockback);
        }
        
        [Test]
        public void KnockbackStateTracking()
        {
            var body = new MOBA.Physics.PhysicsBody();
            body.knockbackDirection = Vector3.forward;
            body.knockbackForce = 10f;
            body.knockbackDuration = 0.5f;
            body.knockbackTimer = 0.3f;
            body.isInKnockback = true;
            
            Assert.AreEqual(Vector3.forward, body.knockbackDirection);
            Assert.AreEqual(10f, body.knockbackForce);
            Assert.AreEqual(0.5f, body.knockbackDuration);
            Assert.AreEqual(0.3f, body.knockbackTimer);
            Assert.IsTrue(body.isInKnockback);
        }
        
        [Test]
        public void MovementPredictionCalculation()
        {
            // Test basic position prediction
            Vector3 initialPos = Vector3.zero;
            Vector3 velocity = new Vector3(5f, 0f, 0f);
            float deltaTime = 0.1f;
            
            Vector3 predictedPos = initialPos + velocity * deltaTime;
            Vector3 expected = new Vector3(0.5f, 0f, 0f);
            
            Assert.AreEqual(expected, predictedPos);
        }
        
        [Test]
        public void GravityApplicationCalculation()
        {
            // Test gravity calculation
            float gravity = -20f;
            float deltaTime = 0.02f; // 50Hz tick
            Vector3 initialVelocity = new Vector3(0f, 5f, 0f);
            
            Vector3 newVelocity = initialVelocity;
            newVelocity.y += gravity * deltaTime;
            
            Assert.AreEqual(4.6f, newVelocity.y, 0.01f);
        }
        
        [Test]
        public void GroundCollisionDetection()
        {
            // Test ground height collision
            float groundHeight = 0f;
            Vector3 position = new Vector3(0f, -0.5f, 0f); // Below ground
            
            bool belowGround = position.y < groundHeight;
            Assert.IsTrue(belowGround);
            
            // Correct position
            if (belowGround)
            {
                position.y = groundHeight;
            }
            
            Assert.AreEqual(0f, position.y);
        }
    }
}
