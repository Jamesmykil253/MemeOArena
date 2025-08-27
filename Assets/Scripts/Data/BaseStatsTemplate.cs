using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Defines the base statistics and jump physics for an archetype.  Multiple
    /// heroes can reference the same template to share common stats.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/BaseStatsTemplate", fileName = "BaseStatsTemplate", order = 1)]
    public class BaseStatsTemplate : ScriptableObject
    {
        [Header("Core Stats")]
        public float MaxHP = 10000f;
        public float Attack = 100f;
        public float Defense = 0f;
        public float MoveSpeed = 5f;

        [Header("Jump Physics")]
        public JumpPhysicsDef JumpPhysics;
    }
}