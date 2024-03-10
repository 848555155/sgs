using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 马术-锁定技，当你计算与其他角色的距离时，始终-1。
/// </summary>
public class MaShu : PassiveSkill
{
    protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
    {
        owner[Player.RangeMinus]--;
    }

    protected override void UninstallTriggers(Player owner)
    {
        owner[Player.RangeMinus]++;
    }
    public MaShu()
    {
        IsEnforced = true;
    }
}
