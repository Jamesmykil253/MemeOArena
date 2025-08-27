using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// ScriptableObject for basic player stats such as health, attack, defense and move speed.
    /// In the real game, this would include jump physics and more.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/BaseStatsTemplate")]
    public class BaseStatsTemplate : ScriptableObject
    {
        public int MaxHP;
        public int Attack;
        public int Defense;
        public float MoveSpeed;
        public float MagicDefense;

        // Legacy properties for backwards compatibility
        public int maxHP => MaxHP;
        public int attack => Attack;
        public int defense => Defense;
        public float moveSpeed => MoveSpeed;
    }
}