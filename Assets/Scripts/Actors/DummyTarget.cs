using UnityEngine;

namespace MOBA.Actors
{
    /// <summary>
    /// A simple target object with health that can take damage. When health
    /// reaches zero it logs a message and can be extended to trigger other
    /// events (e.g., respawn, scoring).
    /// </summary>
    public class DummyTarget : MonoBehaviour
    {
        public float maxHP = 100f;
        public float currentHP;

        private void Awake()
        {
            currentHP = maxHP;
        }

        /// <summary>
        /// Apply damage to the dummy target. Clamps HP at zero and logs when dead.
        /// </summary>
        /// <param name="damage">Amount of damage to apply.</param>
        public void TakeDamage(int damage)
        {
            currentHP -= damage;
            if (currentHP <= 0f)
            {
                currentHP = 0f;
                Debug.Log($"{name} has been defeated.");
                // Extend here: award points, respawn, etc.
            }
        }

        /// <summary>
        /// For demonstration: print current HP in the editor on click.
        /// </summary>
        private void OnMouseDown()
        {
            Debug.Log($"{name} HP: {currentHP}/{maxHP}");
        }
    }
}
