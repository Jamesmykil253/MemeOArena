using System.Collections.Generic;
using UnityEngine;
using MOBA.Core;

namespace MOBA.Data
{
    /// <summary>
    /// Holds the runtime data for a player.  This context is passed to various
    /// systems and state machines.  It stores references to ScriptableObjects
    /// (templates) and mutable state such as current health, carried points and
    /// energy.  Use this class to encapsulate all per‑player variables.
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
        /// Reset runtime stats based on the base template and level.  Call this
        /// when spawning or respawning.
        /// </summary>
        public void ResetToTemplate()
        {
            if (baseStats != null)
            {
                currentHP = baseStats.MaxHP;
                attack = baseStats.Attack;
                defense = baseStats.Defense;
                moveSpeed = baseStats.MoveSpeed;
            }
            carriedPoints = 0;
            ultimateEnergy = 0f;
            ultimateCooldownRemaining = 0f;
            ultimateReady = false;
        }

        /// <summary>
        /// Apply damage to the player after defense mitigation.  Damage cannot
        /// reduce HP below zero.
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHP -= damage;
            if (currentHP < 0f)
            {
                currentHP = 0f;
            }
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