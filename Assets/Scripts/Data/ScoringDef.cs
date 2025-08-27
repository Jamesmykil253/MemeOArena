using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Contains the data used to compute channel times for scoring.  Base
    /// channel times are defined for brackets of points carried.  Additive
    /// speed factors represent buffs (e.g. items, objectives).  Ally synergy
    /// multipliers reduce the channel time multiplicatively based on the
    /// number of allies present on the scoring pad.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/ScoringDef", fileName = "ScoringDef", order = 5)]
    public class ScoringDef : ScriptableObject
    {
        [Header("Base Times")]
        [Tooltip("Thresholds for points carried.  If points <= threshold[i], use baseTimes[i].")]
        public int[] thresholds = new int[] { 6, 12, 18, 24, 33 };
        [Tooltip("Base channel times corresponding to the thresholds above.")]
        public float[] baseTimes = new float[] { 0.5f, 1.0f, 1.5f, 2.0f, 3.0f };

        [Header("Ally Synergy")] 
        [Tooltip("Multiplicative factors applied to channel time for 0,1,2,3,4 allies (including the carrier).")]
        public float[] allySynergyMultipliers = new float[] { 1.0f, 0.70f, 0.65f, 0.60f, 0.40f };

        [Header("Additive Speed Factors")]
        [Tooltip("List of additive speed factors from buffs or items.  Keys are identifiers used at runtime.")]
        public List<SpeedFactor> additiveFactors = new List<SpeedFactor>();

        [System.Serializable]
        public class SpeedFactor
        {
            public string id;
            public float value;
        }

        /// <summary>
        /// Get the base channel time based on the number of points carried.
        /// </summary>
        public float GetBaseTime(int points)
        {
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (points <= thresholds[i])
                {
                    return baseTimes[i];
                }
            }
            // If points exceed the last threshold, extrapolate using the last base time
            return baseTimes[baseTimes.Length - 1];
        }

        /// <summary>
        /// Sum additive speed factors for the given set of active buff IDs.
        /// </summary>
        public float SumSpeedFactors(IEnumerable<string> activeBuffIds)
        {
            float sum = 0f;
            foreach (var buff in additiveFactors)
            {
                foreach (var id in activeBuffIds)
                {
                    if (buff.id == id)
                    {
                        sum += buff.value;
                    }
                }
            }
            return sum;
        }

        /// <summary>
        /// Get synergy multiplier based on the number of allies present.  The
        /// array index corresponds to the number of allies (0 = solo).
        /// </summary>
        public float GetSynergyMultiplier(int allies)
        {
            if (allies < 0) allies = 0;
            if (allies >= allySynergyMultipliers.Length)
            {
                return allySynergyMultipliers[allySynergyMultipliers.Length - 1];
            }
            return allySynergyMultipliers[allies];
        }
    }
}