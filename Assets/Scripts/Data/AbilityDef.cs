using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Defines a single ability.  Includes cast time, cooldown and RSB
    /// coefficients used by the combat system as well as knockback and input key.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/AbilityDef", fileName = "AbilityDef", order = 3)]
    public class AbilityDef : ScriptableObject
    {
        [Header("Timing")]
        public float CastTime = 0.25f;
        public float Cooldown = 5f;

        [Header("RSB Coefficients")]
        public float Ratio = 1f; // R in the RSB formula
        public float Slider = 0f; // S in the RSB formula
        public float Base = 0f; // B in the RSB formula

        [Header("Effects")]
        public float Knockback = 0f;

        [Header("Input")]
        public KeyCode InputKey = KeyCode.None;
    }
}