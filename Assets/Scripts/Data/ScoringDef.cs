using System;
using UnityEngine;

namespace MOBA.Data
{
    // ScriptableObject defining thresholds and deposit times for scoring orbs.
    [CreateAssetMenu(menuName = "Game/ScoringDef")]
    public class ScoringDef : ScriptableObject
    {
        public int[] thresholds;
        public float[] baseTimes;
        public float[] additiveSpeedFactors;
        public float[] synergyMultipliers;

        /// <summary>
        /// Test-facing method to get the base time for a given tier.
        /// </summary>
        public float GetBaseTime(int tier)
        {
            if (baseTimes == null || tier < 0 || tier >= baseTimes.Length)
                return 0f;
            return baseTimes[tier];
        }

        /// <summary>
        /// Test-facing method to get the base time for a given number of points.
        /// </summary>
        public float GetBaseTime(int points, int[] thresholds = null)
        {
            var thresholdsToUse = thresholds ?? this.thresholds;
            if (thresholdsToUse == null || baseTimes == null) return 0f;
            
            // VALIDATION: Check array length mismatch
            if (thresholdsToUse.Length != baseTimes.Length)
            {
                throw new System.InvalidOperationException($"Threshold array length ({thresholdsToUse.Length}) does not match baseTime array length ({baseTimes.Length})");
            }
            
            // VALIDATION: Check if thresholds are sorted
            for (int i = 1; i < thresholdsToUse.Length; i++)
            {
                if (thresholdsToUse[i] < thresholdsToUse[i - 1])
                {
                    throw new System.InvalidOperationException($"Thresholds must be sorted in ascending order. Found {thresholdsToUse[i - 1]} > {thresholdsToUse[i]} at index {i}");
                }
            }
            
            int tier = 0;
            for (int i = 0; i < thresholdsToUse.Length; i++)
            {
                if (points >= thresholdsToUse[i])
                    tier = i;
                else
                    break;
            }
            return GetBaseTime(tier);
        }

        /// <summary>
        /// Test-facing method to sum all speed factors.
        /// </summary>
        public float SumSpeedFactors()
        {
            if (additiveSpeedFactors == null) return 0f;
            
            float sum = 0f;
            for (int i = 0; i < additiveSpeedFactors.Length; i++)
            {
                sum += additiveSpeedFactors[i];
            }
            return sum;
        }

        /// <summary>
        /// Test-facing method to sum speed factors for active buffs.
        /// </summary>
        public float SumSpeedFactors(System.Collections.Generic.IEnumerable<string> activeBuffs)
        {
            return SumSpeedFactors(); // Simplified for now
        }

        /// <summary>
        /// Test-facing method to get synergy multiplier for a given tier.
        /// </summary>
        public float GetSynergyMultiplier(int tier)
        {
            if (synergyMultipliers == null || tier < 0 || tier >= synergyMultipliers.Length)
                return 1f;
            return synergyMultipliers[tier];
        }
    }
}