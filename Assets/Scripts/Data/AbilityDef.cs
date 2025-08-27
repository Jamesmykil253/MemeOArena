using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Defines a single ability.  Includes cast time, cooldown and RSB
    /// coefficients used by the combat system as well as knockback, input key,
    /// critical hit parameters, defense penetration and damage type.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/AbilityDef", fileName = "AbilityDef", order = 3)]
    public class AbilityDef : ScriptableObject
    {
        /// <summary>
        /// Enumerates the types of damage an ability can deal.
        /// </summary>
        public enum DamageType
        {
            Physical,
            Magical,
            True
        }

        [Header("Timing")]
        public float CastTime = 0.25f;
        public float Cooldown = 5f;

        [Header("Resource Cost")]
        public float energyCost = 0f;

        [Header("RSB Coefficients")]
        public float Ratio = 1f; // R in the RSB formula
        public float Slider = 0f; // S in the RSB formula
        public float Base = 0f; // B in the RSB formula

        [Header("Effects")]
        public float Knockback = 0f;

        [Header("Input")]
        public KeyCode InputKey = KeyCode.None;

        [Header("Critical & Penetration")]
        [Tooltip("Chance for this ability to critically hit (0 to 1).")]
        public float CritChance = 0f;
        [Tooltip("Multiplier applied to damage on a critical hit.")]
        public float CritMultiplier = 1.5f;
        [Tooltip("Flat amount of defense penetration applied before mitigation.")]
        public float DefensePenetration = 0f;

        [Header("Damage Type")]
        [Tooltip("The category of damage this ability deals.")]
        public DamageType damageType = DamageType.Physical;
    }
}
