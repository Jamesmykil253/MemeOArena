using UnityEngine;

/// <summary>
/// ScriptableObject for basic player stats such as health, attack, defense and move speed.
/// In the real game, this would include jump physics and more.
/// </summary>
[CreateAssetMenu(menuName = "Game/BaseStatsTemplate")]
public class BaseStatsTemplate : ScriptableObject
{
    public int maxHP;
    public int attack;
    public int defense;
    public float moveSpeed;
}