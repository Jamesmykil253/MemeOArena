using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Defines match settings such as length, team size and other mode
    /// modifiers.  Designers can create multiple match modes (e.g. casual,
    /// competitive) by creating different ScriptableObjects of this type.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/MatchModeDef", fileName = "MatchModeDef", order = 4)]
    public class MatchModeDef : ScriptableObject
    {
        public float MatchLength = 600f; // seconds
        public int TeamSize = 5;
        // Additional milestone events or rules can be added here
    }
}