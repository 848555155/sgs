using Sanguosha.Core.Triggers;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 救援-主公技，锁定技，当其它吴势力角色在你濒死状态下对你使用【桃】时，你额外回复1点体力。
/// </summary>
public class JiuYuan : TriggerSkill
{
    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        eventArgs.ReadonlyCard[Tao.EatOneGetAnotherFreeCoupon] = 1;
    }

    public JiuYuan()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.Source != null && a.Source.Allegiance == Core.Heroes.Allegiance.Wu && a.ReadonlyCard.Type is Tao && !a.Targets.Contains(a.Source); },
            Run,
            TriggerCondition.OwnerIsTarget
        );

        Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
        IsEnforced = true;
        IsRulerOnly = true;
    }

}
