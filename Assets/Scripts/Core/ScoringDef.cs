using System;
using UnityEngine;

// ScriptableObject defining thresholds and deposit times for scoring orbs.
[CreateAssetMenu(menuName = "Game/ScoringDef")]
public class ScoringDef : ScriptableObject
{
    public int[] thresholds;
    public float[] baseTimes;
    public float[] additiveSpeedFactors;
    public float[] synergyMultipliers;

    /// <summary>
    /// Validate that thresholds and baseTimes arrays align and thresholds ascend.
    /// Throws an exception if the asset is misconfigured.
    /// </summary>
    private void OnValidate()
    {
        if (thresholds != null && baseTimes != null)
        {
            if (thresholds.Length != baseTimes.Length)
            {
                throw new InvalidOperationException($"{name}: thresholds and baseTimes must have the same length.");
            }
            for (int i = 1; i < thresholds.Length; i++)
            {
                if (thresholds[i] < thresholds[i - 1])
                {
                    throw new InvalidOperationException($"{name}: thresholds must be non-decreasing.");
                }
            }
        }
    }
}