using UnityEngine;
using MOBA.Data;

namespace MOBA.Combat
{
    /// <summary>
    /// Provides utility methods for computing damage using the RSB model and
    /// defense mitigation.  This system is stateless and can be unit tested.
    /// </summary>
    public static class CombatSystem
    {
        /// <summary>
        /// Compute raw damage given ability coefficients and attacker stats.
        /// </summary>
        /// <param name="ability">The ability definition containing R, S and B.</param>
        /// <param name="attackStat">The attacker’s relevant stat.</param>
        /// <param name="level">The attacker’s level.</param>
        /// <returns>Raw damage before mitigation.</returns>
        public static int ComputeRawDamage(AbilityDef ability, float attackStat, int level)
        {
            float raw = ability.Ratio * attackStat + ability.Slider * (level - 1) + ability.Base;
            return Mathf.FloorToInt(raw);
        }

        /// <summary>
        /// Compute damage taken after applying defense mitigation.
        /// </summary>
        /// <param name="rawDamage">Raw damage from ComputeRawDamage.</param>
        /// <param name="defense">Defender’s defense stat.</param>
        /// <returns>Final damage taken.</returns>
        public static int ComputeDamageTaken(int rawDamage, float defense)
        {
            float mitigated = rawDamage * 600f / (600f + defense);
            return Mathf.FloorToInt(mitigated);
        }

        /// <summary>
        /// Apply flat damage reduction after defense mitigation.  The reduction
        /// parameter should be between 0 (no reduction) and 1 (full reduction).
        /// Multiple reductions stack additively.
        /// </summary>
        public static int ApplyFlatReduction(int damage, float flatReduction)
        {
            float reduced = damage * (1f - flatReduction);
            return Mathf.FloorToInt(reduced);
        }

        /// <summary>
        /// Compute effective HP for a unit given its MaxHP and Defense.
        /// </summary>
        public static float ComputeEffectiveHP(float maxHP, float defense)
        {
            return maxHP * (1f + defense / 600f);
        }
    }
}