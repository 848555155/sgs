﻿using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Woods.Skills;

/// <summary>
/// 帷幕-锁定技，你不能成为黑色锦囊牌的目标。
/// </summary>
public class WeiMu : TriggerSkill
{
    public WeiMu()
    {
        Triggers.Add(GameEvent.PlayerCanBeTargeted, new RelayTrigger(
            (p, e, a) =>
            {
                return CardCategoryManager.IsCardCategory(a.Card.Type.Category, CardCategory.Tool) && a.Card.SuitColor == SuitColorType.Black;
            },
            (p, e, a) =>
            {
                if (a.Card.Place.DeckType == DeckType.DelayedTools)
                {
                    NotifySkillUse();
                }
                throw new TriggerResultException(TriggerResult.Fail);
            },
            TriggerCondition.OwnerIsTarget
            ));

        var notify = new AutoNotifyPassiveSkillTrigger(
             this,
             (p, e, a) => { return a.Source != p && a.ReadonlyCard.Type is Aoe && a.ReadonlyCard.SuitColor == SuitColorType.Black; },
             (p, e, a) => { },
             TriggerCondition.Global
         );
        Triggers.Add(GameEvent.PlayerUsedCard, notify);
        IsEnforced = true;
    }

}
