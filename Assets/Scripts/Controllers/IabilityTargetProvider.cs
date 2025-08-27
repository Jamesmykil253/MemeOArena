using MOBA.Data;
using MOBA.Combat;

namespace MOBA.Controllers
{
    /// <summary>
    /// Strategy interface for resolving the primary target for an ability.
    /// </summary>
    public interface IAbilityTargetProvider
    {
        IDamageable FindPrimaryTarget(PlayerContext ctx);
    }
}
