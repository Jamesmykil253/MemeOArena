using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Defines energy gains and global constants for ultimate abilities.  Energy
    /// values are arbitrary units; designers can tune them to achieve desired
    /// ultimate cadence.  See docs/UltimateEnergy.md for design guidance.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/UltimateEnergyDef", fileName = "UltimateEnergyDef", order = 6)]
    public class UltimateEnergyDef : ScriptableObject
    {
        [Header("Energy Sources")]
        public float passiveRegenPerSecond = 900f;
        public float neutralKillEnergy = 5000f;
        public float scoreDepositEnergy = 12000f;
        public float koEnergy = 12000f;

        [Header("Global Constants")]
        public float cooldownConstant = 900f;

        [Header("Ultimate Requirements")]
        public float energyRequirement = 90000f;
    }
}