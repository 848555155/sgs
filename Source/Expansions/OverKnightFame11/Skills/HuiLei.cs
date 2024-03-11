﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.OverKnightFame11.Skills;

/// <summary>
/// 挥泪-锁定技，当你被其他角色杀死时，该角色弃置其所有的牌。
/// </summary>
public class HuiLei : TriggerSkill
{
    public HuiLei()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.Source != null; },
            (p, e, a) =>
            {
                List<Card> toDiscard = new List<Card>();
                toDiscard.AddRange(a.Source.Equipments());
                toDiscard.AddRange(a.Source.HandCards());
                Game.CurrentGame.HandleCardDiscard(a.Source, toDiscard);
            },
            TriggerCondition.OwnerIsTarget
        );

        Triggers.Add(GameEvent.PlayerIsDead, trigger);
        IsEnforced = true;
    }
}
