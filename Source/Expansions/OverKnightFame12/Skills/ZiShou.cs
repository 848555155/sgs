﻿using Sanguosha.Core.Triggers;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.OverKnightFame12.Skills;

/// <summary>
/// 自守-摸牌阶段，若你已受伤，你可以额外摸X张牌（X为你已损失的体力值），然后跳过你的出牌阶段。
/// </summary>
public class ZiShou : TriggerSkill
{
    public ZiShou()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                return p.LostHealth > 0;
            },
            (p, e, a) =>
            {
                p[Player.DealAdjustment] += p.LostHealth;
                Game.CurrentGame.PhasesSkipped.Add(TurnPhase.Play);
            },
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
        IsAutoInvoked = null;
    }
}
