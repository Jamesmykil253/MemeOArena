using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Minimal player context stub used by tests to hold references to base stats and ultimate energy definitions.
    /// In a real project this would also track current HP, ability cooldowns, and more.
    /// </summary>
    public class PlayerContext
    {
        public string playerId;
        public BaseStatsTemplate baseStats;
        public float ultimateEnergy;
        public UltimateEnergyDef ultimateDef;
        public ScoringDef scoringDef;
        public List<AbilityDef> abilities = new List<AbilityDef>();
        public float moveSpeed => baseStats?.MoveSpeed ?? 0f;
        
        // Runtime stats
        public int currentHP;
        public float magicDefense = 0f;
        public float ultimateCooldownRemaining = 0f;
        
        // Combat stats (derived from base stats)
        public int level = 1; // Default level
        public float currentAttack => baseStats?.Attack ?? 0f;
        
        // Scoring state
        public int currentScore = 0;
        public int carriedPoints = 0;
        public bool isCarrying = false;
        public bool isChanneling = false;
        public bool ultimateReady = false;
        public float channelStartTime = 0f;

        public PlayerContext(string id, BaseStatsTemplate stats, UltimateEnergyDef ultimate, ScoringDef scoring)
        {
            playerId = id;
            baseStats = stats;
            ultimateDef = ultimate;
            scoringDef = scoring;
            
            // Initialize runtime stats from template
            if (stats != null)
            {
                currentHP = stats.MaxHP;
                magicDefense = stats.MagicDefense;
            }
        }

        /// <summary>
        /// Apply damage to the player, reducing current HP.
        /// </summary>
        public void ApplyDamage(int damage)
        {
            currentHP = Mathf.Max(0, currentHP - damage);
        }

        /// <summary>
        /// Reset runtime stats to match the base template.
        /// </summary>
        public void ResetToTemplate()
        {
            if (baseStats != null)
            {
                currentHP = baseStats.MaxHP;
                magicDefense = baseStats.MagicDefense;
            }
        }
    }
}