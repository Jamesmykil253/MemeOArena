using UnityEngine;
using MOBA.Data;

namespace MOBA.Bootstrap
{
    /// <summary>
    /// Utility class to create default ScriptableObject assets for testing and demo purposes.
    /// This allows us to have working game data without manually creating assets in the Unity editor.
    /// </summary>
    public static class DefaultAssetCreator
    {
        /// <summary>
        /// Creates a default BaseStatsTemplate for a balanced player character.
        /// </summary>
        public static BaseStatsTemplate CreateDefaultPlayerStats()
        {
            var template = ScriptableObject.CreateInstance<BaseStatsTemplate>();
            template.MaxHP = 1000;
            template.Attack = 100;
            template.Defense = 50;
            template.MoveSpeed = 5.5f;
            template.MagicDefense = 30f;
            return template;
        }

        /// <summary>
        /// Creates a default AbilityDef for a basic attack ability.
        /// </summary>
        public static AbilityDef CreateDefaultBasicAbility()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDef>();
            ability.CastTime = 0.3f;
            ability.Cooldown = 2f;
            ability.energyCost = 0f;
            ability.Ratio = 1.2f; // 120% attack scaling
            ability.Slider = 15f; // +15 damage per level
            ability.Base = 80f; // 80 base damage
            ability.Knockback = 2f;
            ability.InputKey = KeyCode.Q;
            ability.CritChance = 0.15f; // 15% crit chance
            ability.CritMultiplier = 1.5f; // 150% crit damage
            ability.DefensePenetration = 0f;
            ability.damageType = AbilityDef.DamageType.Physical;
            return ability;
        }

        /// <summary>
        /// Creates a default AbilityDef for an ultimate ability.
        /// </summary>
        public static AbilityDef CreateDefaultUltimateAbility()
        {
            var ultimate = ScriptableObject.CreateInstance<AbilityDef>();
            ultimate.CastTime = 0.8f;
            ultimate.Cooldown = 60f; // Long cooldown, managed by energy system
            ultimate.energyCost = 100f;
            ultimate.Ratio = 2.5f; // 250% attack scaling
            ultimate.Slider = 50f; // +50 damage per level
            ultimate.Base = 300f; // 300 base damage
            ultimate.Knockback = 8f; // Strong knockback
            ultimate.InputKey = KeyCode.R;
            ultimate.CritChance = 0.25f; // 25% crit chance
            ultimate.CritMultiplier = 2f; // 200% crit damage
            ultimate.DefensePenetration = 25f; // Penetrates 25 defense
            ultimate.damageType = AbilityDef.DamageType.Physical;
            return ultimate;
        }

        /// <summary>
        /// Creates default jump physics for responsive movement.
        /// </summary>
        public static JumpPhysicsDef CreateDefaultJumpPhysics()
        {
            var jumpPhysics = ScriptableObject.CreateInstance<JumpPhysicsDef>();
            jumpPhysics.BaseJumpVelocity = 12f; // Strong initial jump
            jumpPhysics.Gravity = -25f; // Responsive gravity
            jumpPhysics.CoyoteTime = 0.15f; // Generous coyote time
            jumpPhysics.DoubleJumpWindow = 0.3f; // Reasonable double-jump window
            jumpPhysics.NormalJumpMultiplier = 1.0f;
            jumpPhysics.HighJumpMultiplier = 1.5f;
            jumpPhysics.DoubleJumpMultiplier = 2.0f;
            jumpPhysics.ApexBoostMultiplier = 1.8f;
            jumpPhysics.AllowDoubleJump = true;
            jumpPhysics.EnableApexBoost = true;
            jumpPhysics.MinHoldTime = 0.2f;
            jumpPhysics.MaxHoldTime = 1.0f;
            jumpPhysics.ApexWindow = 0.3f;
            return jumpPhysics;
        }

        /// <summary>
        /// Creates default ultimate energy settings.
        /// </summary>
        public static UltimateEnergyDef CreateDefaultUltimateEnergy()
        {
            var energyDef = ScriptableObject.CreateInstance<UltimateEnergyDef>();
            energyDef.maxEnergy = 100f;
            energyDef.regenRate = 1.5f; // Slow passive regen
            energyDef.scoreDepositEnergy = 30f; // Good energy from scoring
            energyDef.energyRequirement = 100f; // Energy needed for ultimate
            energyDef.cooldownConstant = 2f; // 50 second cooldown (100/2)
            return energyDef;
        }

        /// <summary>
        /// Creates default scoring settings with balanced progression.
        /// </summary>
        public static ScoringDef CreateDefaultScoring()
        {
            var scoringDef = ScriptableObject.CreateInstance<ScoringDef>();
            
            // Point thresholds: 1-5 points = tier 0, 6-10 = tier 1, 11-15 = tier 2, etc.
            scoringDef.thresholds = new int[] { 5, 10, 15, 20 };
            
            // Base times: quick for small deposits, longer for big deposits
            scoringDef.baseTimes = new float[] { 1f, 2f, 3.5f, 5f, 7f };
            
            // Speed factors: various buffs can add these
            scoringDef.additiveSpeedFactors = new float[] { 0.5f, 1f, 0.3f };
            
            // Synergy multipliers: 1 ally = 80% time, 2 allies = 65% time, 3+ allies = 50% time
            scoringDef.synergyMultipliers = new float[] { 1f, 0.8f, 0.65f, 0.5f };
            
            return scoringDef;
        }

        /// <summary>
        /// Creates a complete set of default assets for testing.
        /// Returns a configuration object with all necessary data.
        /// </summary>
        public static DefaultGameConfig CreateCompleteDefaultConfig()
        {
            return new DefaultGameConfig
            {
                playerStats = CreateDefaultPlayerStats(),
                basicAbility = CreateDefaultBasicAbility(),
                ultimateAbility = CreateDefaultUltimateAbility(),
                jumpPhysics = CreateDefaultJumpPhysics(),
                ultimateEnergy = CreateDefaultUltimateEnergy(),
                scoring = CreateDefaultScoring()
            };
        }
    }

    /// <summary>
    /// Container for a complete set of default game configuration data.
    /// </summary>
    [System.Serializable]
    public class DefaultGameConfig
    {
        public BaseStatsTemplate playerStats;
        public AbilityDef basicAbility;
        public AbilityDef ultimateAbility;
        public JumpPhysicsDef jumpPhysics;
        public UltimateEnergyDef ultimateEnergy;
        public ScoringDef scoring;
    }
}
