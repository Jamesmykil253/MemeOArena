using UnityEngine;
using MOBA.Bootstrap;
using MOBA.Controllers;

namespace MOBA.Demo
{
    /// <summary>
    /// Demo pickup that adds points when the player walks through it
    /// </summary>
    public class DemoPointPickup : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [SerializeField] private int pointValue = 3;
        [SerializeField] private bool respawn = true;
        [SerializeField] private float respawnDelay = 5f;
        [SerializeField] private AudioClip pickupSound;
        
        private bool isActive = true;
        private Renderer pickupRenderer;
        private Collider pickupCollider;
        
        void Start()
        {
            pickupRenderer = GetComponent<Renderer>();
            pickupCollider = GetComponent<Collider>();
            
            // Make sure it's a trigger
            if (pickupCollider != null)
            {
                pickupCollider.isTrigger = true;
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            
            // Check if it's the player
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<UnifiedLocomotionController>();
                var bootstrapper = other.GetComponent<GameBootstrapper>();
                
                if (player != null || bootstrapper != null)
                {
                    CollectPickup(other.gameObject);
                }
            }
        }
        
        private void CollectPickup(GameObject player)
        {
            // Add points to player
            var bootstrapper = player.GetComponent<GameBootstrapper>();
            if (bootstrapper != null)
            {
                bootstrapper.AddPoints(pointValue);
            }
            
            // Visual/Audio feedback
            PlayPickupEffects();
            
            // Hide pickup
            SetPickupActive(false);
            
            Debug.Log($"Player collected {pointValue} points from {name}!");
            
            // Respawn if enabled
            if (respawn)
            {
                Invoke(nameof(RespawnPickup), respawnDelay);
            }
            else
            {
                // Destroy after brief delay to let effects play
                Destroy(gameObject, 1f);
            }
        }
        
        private void PlayPickupEffects()
        {
            // Play sound if available
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Simple visual effect - could be replaced with particle system
            StartCoroutine(PickupEffect());
        }
        
        private System.Collections.IEnumerator PickupEffect()
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;
            Vector3 startScale = transform.localScale;
            
            float time = 0f;
            float duration = 0.5f;
            
            while (time < duration)
            {
                float t = time / duration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                
                time += Time.deltaTime;
                yield return null;
            }
        }
        
        private void SetPickupActive(bool active)
        {
            isActive = active;
            
            if (pickupRenderer != null)
            {
                pickupRenderer.enabled = active;
            }
            
            if (pickupCollider != null)
            {
                pickupCollider.enabled = active;
            }
        }
        
        private void RespawnPickup()
        {
            // Reset position and scale
            transform.localScale = Vector3.one * 0.7f;
            
            // Reactivate
            SetPickupActive(true);
            
            Debug.Log($"{name} respawned!");
        }
        
        void Update()
        {
            if (isActive)
            {
                // Add a gentle pulse effect
                float pulse = Mathf.Sin(Time.time * 3f) * 0.1f + 1f;
                transform.localScale = Vector3.one * 0.7f * pulse;
            }
        }
    }
}
