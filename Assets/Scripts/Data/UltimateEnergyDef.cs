using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// ScriptableObject describing the energy requirements for an ultimate ability: max, regen rate and required amount.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/UltimateEnergyDef")]
    public class UltimateEnergyDef : ScriptableObject
    {
        public float maxEnergy;
        public float regenRate;
        public float required;
        public float scoreDepositEnergy;
        public float energyRequirement;
        public float cooldownConstant = 50f; // Default cooldown constant
    }
}