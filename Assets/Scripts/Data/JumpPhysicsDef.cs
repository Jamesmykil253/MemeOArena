using UnityEngine;

namespace MOBA.Data
{
    /// <summary>
    /// Holds deterministic jump parameters.  The physics system samples these
    /// values at a fixed timestep to ensure reproducible trajectories.
    /// </summary>
    [CreateAssetMenu(menuName = "MOBA/Data/JumpPhysicsDef", fileName = "JumpPhysicsDef", order = 2)]
    public class JumpPhysicsDef : ScriptableObject
    {
        public float InitialVelocity = 10f;
        public float Gravity = -20f;
        public float CoyoteTime = 0.1f; // time after leaving ground when jump is still allowed
        public float DoubleJumpWindow = 0.2f; // window after first jump where a double jump is allowed
    }
}