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
        public void UnityPhysicsSettingsWork()
        {
            // Test Unity's default rigidbody settings
            var testGO = new GameObject("PhysicsTest");
            var rb = testGO.AddComponent<Rigidbody>();
            
            Assert.AreEqual(1f, rb.mass);
            Assert.IsTrue(rb.useGravity);
            Assert.IsFalse(rb.isKinematic);
            Assert.AreEqual(0f, rb.linearDamping);
            Assert.AreEqual(0f, rb.angularDamping);
            
            UnityEngine.Object.DestroyImmediate(testGO);
        }
        
        [Test]
        public void UnityRigidbodyInitializationWorks()
        {
            var testGO = new GameObject("PhysicsTest");
            var body = testGO.AddComponent<Rigidbody>();
            
            body.position = new Vector3(1f, 2f, 3f);
            body.linearVelocity = new Vector3(0.5f, 0f, 0.2f);
            
            Assert.AreEqual(new Vector3(1f, 2f, 3f), body.position);
            Assert.AreEqual(new Vector3(0.5f, 0f, 0.2f), body.linearVelocity);
            
            UnityEngine.Object.DestroyImmediate(testGO);
        }
        
        [Test]
        public void UnityRigidbodyKnockbackSimulation()
        {
            var testGO = new GameObject("KnockbackTest");
            var body = testGO.AddComponent<Rigidbody>();
            
            // Simulate knockback by applying force
            Vector3 knockbackDirection = Vector3.forward;
            float knockbackForce = 10f;
            
            body.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            
            Assert.AreEqual(Vector3.forward * knockbackForce, body.linearVelocity);
            
            UnityEngine.Object.DestroyImmediate(testGO);
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
