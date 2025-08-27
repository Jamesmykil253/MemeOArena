using UnityEngine;
using MOBA.Combat;
using MOBA.Data;
using MOBA.Actors; // assumes DummyTarget : MonoBehaviour, IDamageable

namespace MOBA.Controllers
{
    /// <summary>
    /// Back-compat provider that uses the modern, faster API.
    /// Replace with an injected, deterministic provider for shipping.
    /// </summary>
    public sealed class FirstObjectDummyTargetProvider : IAbilityTargetProvider
    {
        public static readonly FirstObjectDummyTargetProvider Instance = new FirstObjectDummyTargetProvider();
        private FirstObjectDummyTargetProvider() { }

        public IDamageable FindPrimaryTarget(PlayerContext ctx)
        {
#if UNITY_2023_1_OR_NEWER
            return (IDamageable)Object.FindFirstObjectByType<DummyTarget>(FindObjectsInactive.Exclude);
#else
            return Object.FindObjectOfType<DummyTarget>();
#endif
        }
    }
}
