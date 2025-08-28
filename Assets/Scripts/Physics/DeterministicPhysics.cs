using System;
using UnityEngine;
using MOBA.Core;
using MOBA.Data;
using MOBA.Telemetry;

namespace MOBA.Physics
{
    /// <summary>
    /// Deterministic physics system for precise multiplayer simulation.
    /// Handles movement, collisions, and physics state synchronization.
    /// </summary>
    public class DeterministicPhysics : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float gravityForce = -20f;
        [SerializeField] private float groundHeight = 0f;
        [SerializeField] private float airResistance = 0.95f;
        [SerializeField] private float groundFriction = 0.9f;
        [SerializeField] private bool useFixedTimestep = true;
        
        [Header("Collision Detection")]
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private LayerMask wallLayer = 1;
        [SerializeField] private float skinWidth = 0.01f;
        // maxCollisionIterations will be added when collision iteration limiting is implemented
        
        // Physics state
        private readonly System.Collections.Generic.Dictionary<string, PhysicsBody> bodies = 
            new System.Collections.Generic.Dictionary<string, PhysicsBody>();
        
        private TickManager tickManager;
        private float fixedDeltaTime;
        
        public event Action<string, Vector3, Vector3> OnBodyMoved; // bodyId, oldPos, newPos
        // OnBodyCollision event will be added when collision detection system is fully implemented
        
        public float GravityForce => gravityForce;
        public float GroundHeight => groundHeight;
        
        private void Awake()
        {
            tickManager = FindFirstObjectByType<TickManager>();
        }
        
        private void Start()
        {
            fixedDeltaTime = useFixedTimestep && tickManager != null 
                ? tickManager.TickInterval 
                : Time.fixedDeltaTime;
            
            if (tickManager != null)
            {
                TickManager.OnFixedUpdate += OnFixedUpdate;
            }
            
            GameLogger.LogGameplayEvent(0, "PHYSICS", "INIT", 
                $"Deterministic physics initialized, dt:{fixedDeltaTime:F4}");
        }
        
        private void OnFixedUpdate(float deltaTime)
        {
            // Override deltaTime with fixed timestep for determinism
            float dt = useFixedTimestep ? fixedDeltaTime : deltaTime;
            
            // Update all physics bodies
            foreach (var body in bodies.Values)
            {
                UpdatePhysicsBody(body, dt);
            }
        }
        
        /// <summary>
        /// Register a physics body for simulation
        /// </summary>
        public void RegisterBody(string bodyId, Vector3 position, Vector3 velocity, 
                                PhysicsBodySettings settings)
        {
            PhysicsBody body = new PhysicsBody
            {
                id = bodyId,
                position = position,
                velocity = velocity,
                settings = settings,
                isGrounded = position.y <= groundHeight + skinWidth,
                lastPosition = position
            };
            
            bodies[bodyId] = body;
            
            GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, "PHYSICS", "BODY_REGISTERED", bodyId);
        }
        
        /// <summary>
        /// Unregister a physics body
        /// </summary>
        public void UnregisterBody(string bodyId)
        {
            if (bodies.Remove(bodyId))
            {
                GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, "PHYSICS", "BODY_UNREGISTERED", bodyId);
            }
        }
        
        /// <summary>
        /// Get physics body by ID
        /// </summary>
        public PhysicsBody GetBody(string bodyId)
        {
            return bodies.TryGetValue(bodyId, out PhysicsBody body) ? body : null;
        }
        
        /// <summary>
        /// Set body velocity (for external forces/input)
        /// </summary>
        public void SetBodyVelocity(string bodyId, Vector3 velocity)
        {
            if (bodies.TryGetValue(bodyId, out PhysicsBody body))
            {
                body.velocity = velocity;
            }
        }
        
        /// <summary>
        /// Apply force to a body
        /// </summary>
        public void ApplyForce(string bodyId, Vector3 force)
        {
            if (bodies.TryGetValue(bodyId, out PhysicsBody body))
            {
                // F = ma, so a = F/m
                Vector3 acceleration = force / body.settings.mass;
                body.velocity += acceleration * fixedDeltaTime;
                
                GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, bodyId, "FORCE_APPLIED", 
                    $"Force:{force} NewVel:{body.velocity}");
            }
        }
        
        /// <summary>
        /// Apply impulse (instant velocity change) to a body
        /// </summary>
        public void ApplyImpulse(string bodyId, Vector3 impulse)
        {
            if (bodies.TryGetValue(bodyId, out PhysicsBody body))
            {
                // Impulse = change in momentum = mass * change in velocity
                Vector3 deltaV = impulse / body.settings.mass;
                body.velocity += deltaV;
                
                GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, bodyId, "IMPULSE_APPLIED", 
                    $"Impulse:{impulse} DeltaV:{deltaV}");
            }
        }
        
        /// <summary>
        /// Perform knockback on a body
        /// </summary>
        public void ApplyKnockback(string bodyId, Vector3 direction, float force, float duration)
        {
            if (bodies.TryGetValue(bodyId, out PhysicsBody body))
            {
                body.knockbackDirection = direction.normalized;
                body.knockbackForce = force;
                body.knockbackDuration = duration;
                body.knockbackTimer = duration;
                body.isInKnockback = true;
                
                GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, bodyId, "KNOCKBACK_START", 
                    $"Direction:{direction} Force:{force} Duration:{duration}");
            }
        }
        
        private void UpdatePhysicsBody(PhysicsBody body, float dt)
        {
            Vector3 oldPosition = body.position;
            
            // Handle knockback
            if (body.isInKnockback)
            {
                UpdateKnockback(body, dt);
            }
            else
            {
                // Apply gravity if not grounded
                if (!body.isGrounded && body.settings.useGravity)
                {
                    body.velocity.y += gravityForce * dt;
                }
                
                // Apply air resistance
                if (!body.isGrounded)
                {
                    body.velocity *= airResistance;
                }
                else
                {
                    // Apply ground friction to horizontal movement
                    Vector3 horizontalVel = new Vector3(body.velocity.x, 0f, body.velocity.z);
                    horizontalVel *= groundFriction;
                    body.velocity = new Vector3(horizontalVel.x, body.velocity.y, horizontalVel.z);
                }
            }
            
            // Update position
            body.position += body.velocity * dt;
            
            // Collision detection and response
            HandleCollisions(body);
            
            // Update grounded state
            UpdateGroundedState(body);
            
            // Fire movement event if position changed significantly
            float movementDistance = Vector3.Distance(oldPosition, body.position);
            if (movementDistance > 0.001f)
            {
                OnBodyMoved?.Invoke(body.id, oldPosition, body.position);
                GameMetrics.Instance.RecordMetric("physics_body_movement", movementDistance);
            }
            
            body.lastPosition = oldPosition;
        }
        
        private void UpdateKnockback(PhysicsBody body, float dt)
        {
            body.knockbackTimer -= dt;
            
            if (body.knockbackTimer <= 0f)
            {
                // End knockback
                body.isInKnockback = false;
                GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, body.id, "KNOCKBACK_END", "");
            }
            else
            {
                // Apply knockback force
                float knockbackStrength = (body.knockbackTimer / body.knockbackDuration) * body.knockbackForce;
                Vector3 knockbackVel = body.knockbackDirection * knockbackStrength;
                body.velocity = Vector3.Lerp(body.velocity, knockbackVel, 0.5f);
            }
        }
        
        private void HandleCollisions(PhysicsBody body)
        {
            // Simple collision detection with ground
            if (body.position.y < groundHeight)
            {
                body.position.y = groundHeight;
                if (body.velocity.y < 0f)
                {
                    body.velocity.y = 0f;
                }
                body.isGrounded = true;
            }
            
            // More sophisticated collision detection would go here
            // For now, this handles basic ground collision
        }
        
        private void UpdateGroundedState(PhysicsBody body)
        {
            bool wasGrounded = body.isGrounded;
            body.isGrounded = body.position.y <= groundHeight + skinWidth && body.velocity.y <= 0f;
            
            // Log grounded state changes
            if (wasGrounded != body.isGrounded)
            {
                GameLogger.LogGameplayEvent(tickManager?.CurrentTick ?? 0, body.id, "GROUNDED_CHANGED", 
                    body.isGrounded ? "Landed" : "Airborne");
            }
        }
        
        /// <summary>
        /// Check line of sight between two points
        /// </summary>
        public bool HasLineOfSight(Vector3 from, Vector3 to, LayerMask obstacleLayer)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            
            if (distance < 0.01f) return true;
            
            Ray ray = new Ray(from, direction.normalized);
            return !UnityEngine.Physics.Raycast(ray, distance, obstacleLayer);
        }
        
        /// <summary>
        /// Get all bodies within a radius
        /// </summary>
        public System.Collections.Generic.List<PhysicsBody> GetBodiesInRadius(Vector3 center, float radius)
        {
            var result = new System.Collections.Generic.List<PhysicsBody>();
            
            foreach (var body in bodies.Values)
            {
                float distance = Vector3.Distance(center, body.position);
                if (distance <= radius)
                {
                    result.Add(body);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Predict body position after a given time
        /// </summary>
        public Vector3 PredictPosition(string bodyId, float time)
        {
            if (!bodies.TryGetValue(bodyId, out PhysicsBody body))
                return Vector3.zero;
            
            Vector3 predictedPos = body.position;
            Vector3 predictedVel = body.velocity;
            
            float remainingTime = time;
            float dt = fixedDeltaTime;
            
            while (remainingTime > 0f)
            {
                float stepTime = Mathf.Min(dt, remainingTime);
                
                // Apply gravity if not grounded
                if (predictedPos.y > groundHeight && body.settings.useGravity)
                {
                    predictedVel.y += gravityForce * stepTime;
                }
                
                // Update position
                predictedPos += predictedVel * stepTime;
                
                // Simple ground collision
                if (predictedPos.y < groundHeight)
                {
                    predictedPos.y = groundHeight;
                    predictedVel.y = 0f;
                }
                
                remainingTime -= stepTime;
            }
            
            return predictedPos;
        }
        
        private void OnDestroy()
        {
            if (tickManager != null)
            {
                TickManager.OnFixedUpdate -= OnFixedUpdate;
            }
        }
        
        // Debug visualization
        private void OnDrawGizmos()
        {
            foreach (var body in bodies.Values)
            {
                // Draw body position
                Gizmos.color = body.isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(body.position, 0.5f);
                
                // Draw velocity
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(body.position, body.position + body.velocity);
                
                // Draw knockback direction if in knockback
                if (body.isInKnockback)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(body.position, body.position + body.knockbackDirection * 2f);
                }
            }
        }
    }
    
    /// <summary>
    /// Physics body representation for deterministic simulation
    /// </summary>
    [Serializable]
    public class PhysicsBody
    {
        public string id;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 lastPosition;
        public PhysicsBodySettings settings;
        
        // State flags
        public bool isGrounded;
        public bool isInKnockback;
        
        // Knockback state
        public Vector3 knockbackDirection;
        public float knockbackForce;
        public float knockbackDuration;
        public float knockbackTimer;
    }
    
    /// <summary>
    /// Settings for physics body behavior
    /// </summary>
    [Serializable]
    public class PhysicsBodySettings
    {
        public float mass = 1f;
        public bool useGravity = true;
        public bool isKinematic = false;
        public float drag = 0f;
        public float angularDrag = 0f;
        public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;
        
        public static PhysicsBodySettings Default => new PhysicsBodySettings();
    }
}
