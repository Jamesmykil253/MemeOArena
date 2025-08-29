using UnityEngine;
using MOBA.Data;

namespace MOBA.Combat
{
    /// <summary>
    /// Combat system implementing the RSB (Ratio-Slider-Base) damage model with defense mitigation.
    /// This system is stateless, deterministic, and thoroughly unit tested.
    /// 
    /// RSB DAMAGE MODEL:
    /// ================
    /// Raw Damage = floor(R × AttackStat + S × (Level - 1) + B)
    /// 
    /// Where:
    /// • R (Ratio): How strongly damage scales with attack stat (typically 0.5-2.0)
    /// • S (Slider): Per-level damage increase (creates level pressure)
    /// • B (Base): Minimum damage floor (ensures abilities remain useful)
    /// 
    /// DEFENSE MITIGATION:
    /// ==================
    /// Damage Taken = floor(Raw Damage × 600 / (600 + Defense))
    /// 
    /// This creates diminishing returns where:
    /// • 0 Defense = 100% damage taken
    /// • 150 Defense = 80% damage taken
    /// • 600 Defense = 50% damage taken
    /// • 1200 Defense = 33% damage taken
    /// 
    /// EFFECTIVE HP CALCULATION:
    /// ========================
    /// Effective HP = Max HP × (1 + Defense / 600)
    /// This allows designers to balance survivability vs damage.
    /// 
    /// DESIGN BENEFITS:
    /// ===============
    /// • Predictable scaling for balance
    /// • No magic numbers (all from ScriptableObjects)
    /// • Deterministic (same inputs = same outputs)
    /// • Performance optimized (static methods, minimal allocations)
    /// • Unit testable (pure functions)
    /// 
    /// USAGE:
    /// ======
    /// int rawDmg = CombatSystem.ComputeRawDamage(abilityDef, attackerAttack, attackerLevel);
    /// int finalDmg = CombatSystem.ComputeDamageTaken(rawDmg, defenderDefense);
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
            if (ability == null)
                throw new System.ArgumentNullException(nameof(ability), "AbilityDef cannot be null");
            
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
            // Guard against extreme values that could cause division issues
            if (float.IsInfinity(defense) || float.IsNaN(defense))
                defense = 0f;
            
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

        /// <summary>
        /// Compute final damage applying RSB formulas, defense penetration and critical hits.
        /// This method is deterministic if a seeded System.Random is provided.
        /// </summary>
        /// <param name="attackStat">Attacker's attack stat.</param>
        /// <param name="level">Attacker's level for the RSB formula.</param>
        /// <param name="ability">Ability definition containing coefficients, crit and penetration.</param>
        /// <param name="defenderDefense">Defender's defense stat before penetration.</param>
        /// <param name="rng">Seeded RNG for deterministic crit evaluation.</param>
        public static int ComputeFinalDamage(float attackStat, int level, AbilityDef ability, float defenderDefense, System.Random rng)
        {
            if (ability == null)
                throw new System.ArgumentNullException(nameof(ability), "AbilityDef cannot be null");
            
            // Compute raw damage
            int raw = ComputeRawDamage(ability, attackStat, level);
            // Apply defense penetration
            float adjustedDefense = defenderDefense - ability.DefensePenetration;
            if (adjustedDefense < 0f) adjustedDefense = 0f;
            // Apply mitigation curve
            int mitigated = ComputeDamageTaken(raw, adjustedDefense);
            // Determine critical hit
            bool crit = false;
            if (ability.CritChance > 0f && rng != null)
            {
                crit = rng.NextDouble() < ability.CritChance;
            }
            if (crit)
            {
                mitigated = Mathf.FloorToInt(mitigated * ability.CritMultiplier);
            }
            return mitigated;
        }
    }
}
