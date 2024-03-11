using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.OverKnightFame11.Skills;

/// <summary>
/// 毅重-锁定技，当你没装备防具时，黑色的【杀】对你无效。
/// </summary>
public class YiZhong : TriggerSkill
{
    public YiZhong()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                return p.Armor() == null && a.ReadonlyCard.SuitColor == SuitColorType.Black && a.ReadonlyCard.Type is Sha;
            },
            (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
            TriggerCondition.OwnerIsTarget
        );
        Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
        IsEnforced = true;
    }
}
