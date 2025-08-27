/// <summary>
/// Minimal player context stub used by tests to hold references to base stats and ultimate energy definitions.
/// In a real project this would also track current HP, ability cooldowns, and more.
/// </summary>
public class PlayerContext
{
    public BaseStatsTemplate baseStats;
    public float ultimateEnergy;
    public UltimateEnergyDef ultimateDef;
}