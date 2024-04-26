﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 空城-锁定技，若你没有手牌，你不能成为【杀】或【决斗】的目标。
/// </summary>
public class KongCheng : TriggerSkill
{
    public KongCheng()
    {
        var notifier = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => a.Cards.Any(c => c.HistoryPlace1.DeckType == DeckType.Hand),
            (p, e, a) => { },
            TriggerCondition.OwnerIsSource | TriggerCondition.SourceHasNoHandCards
        );

        Triggers.Add(GameEvent.PlayerCanBeTargeted, new RelayTrigger(
            (p, e, a) => (a.Card.Type is Sha) || (a.Card.Type is JueDou),
            (p, e, a) => throw new TriggerResultException(TriggerResult.Fail),
            TriggerCondition.OwnerIsTarget | TriggerCondition.OwnerHasNoHandCards
            ));
        Triggers.Add(GameEvent.CardsLost, notifier);
        IsEnforced = true;
    }

}
