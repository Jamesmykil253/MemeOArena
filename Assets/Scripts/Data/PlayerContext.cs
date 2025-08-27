using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;

namespace MOBA.Data
{
    /// <summary>
    /// Holds the runtime data for a player.  This context is passed to 
    /// various systems and state machines.  It stores references to 
    /// ScriptableObjects (templates) and mutable state such as current health, carried points
    /// and energy.  Use this class to encapsulate all per‑player variables.
    /// </summary>
    public class PlayerContext
    {
        public string playerId;
        public BaseStatsTemplate baseStats;
        public List<AbilityDef> abilities = new List<AbilityDef>();
        public UltimateEnergyDef ultimateDef;
        public ScoringDef scoringDef;

        // Runtime variables
        public float currentHP;
        public int level = 1;
        public float defense;
        public float magicDefense;
        public float attack;
        public float moveSpeed;
        public int carriedPoints;
        public float ultimateEnergy;
        public float ultimateCooldownRemaining;
        public bool ultimateReady;

        // For scoring system: track active buffs affecting scoring speed
        public HashSet<string> activeSpeedBuffs = new HashSet<string>();

        public PlayerContext(string id, BaseStatsTemplate template, UltimateEnergyDef ultDef, ScoringDef scoreDef)
        {
            playerId = id;
            baseStats = template;
            ultimateDef = ultDef;
            scoringDef = scoreDef;

            ResetToTemplate();
        }

        /// <summary>
        /// Reset runtime stats based on the base template and level.  Call 
        /// this when spawning or respawning.
        /// </summary>
        public void ResetToTemplate()
        {
            if (baseStats != null)
            {
                currentHP = baseStats.MaxHP;
                attack = baseStats.Attack;
                defense = baseStats.Defense;
                magicDefense = baseStats.MagicDefense;
                moveSpeed = baseStats.MoveSpeed;
            }
            carriedPoints = 0;
            ultimateEnergy = 0f;
            ultimateCooldownRemaining = 0f;
            ultimateReady = false;
        }

        /// <summary>
        /// Apply mitigated damage to the player. Damage should already
        /// include any defense or reduction calculations. HP is clamped
        /// to a minimum of zero.
        /// </summary>
        /// <param name="mitigatedDamage">The final damage amount after mitigation.</param>
        public void ApplyDamage(int mitigatedDamage)
        {
            currentHP -= mitigatedDamage;
            if (currentHP < 0f)
            {
                currentHP = 0f;
            }
        }

        /// <summary>
        /// Obsolete. Use <see cref="ApplyDamage(int)"/> instead.
        /// This method forwards to ApplyDamage for backward compatibility.
        /// </summary>
        [System.Obsolete("Use ApplyDamage(int mitigatedDamage) instead.")]
        public void TakeDamage(int damage)
        {
            ApplyDamage(damage);
        }

        /// <summary>
        /// Heal the player.  HP is clamped to MaxHP.
        /// </summary>
        public void Heal(float amount)
        {
            currentHP += amount;
            if (currentHP > baseStats.MaxHP)
            {
                currentHP = baseStats.MaxHP;
            }
        }
    }
}
