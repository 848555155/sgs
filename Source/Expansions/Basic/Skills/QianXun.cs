using Sanguosha.Core.Triggers;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 谦逊-锁定技，你不能取得游戏的胜利。
/// </summary>
public class QianXun : TriggerSkill
{
    private void OnPlayerCanBeTargeted(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        throw new TriggerResultException(TriggerResult.Fail);
    }

    public QianXun()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return (a.Card.Type is ShunShouQianYang) || (a.Card.Type is LeBuSiShu); },
            OnPlayerCanBeTargeted,
            TriggerCondition.OwnerIsTarget
        ) { IsAutoNotify = false };
        Triggers.Add(GameEvent.PlayerCanBeTargeted, trigger);
        IsEnforced = true;
    }

}
