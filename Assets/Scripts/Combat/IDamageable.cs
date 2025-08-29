using UnityEngine;

namespace MOBA.Combat
{
    /// <summary>
    /// Minimal contract for objects that can take damage.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
        float GetDefense(); // For calculating damage reduction
    }
}
