﻿using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Hills.Skills;

/// <summary>
/// 绝境-锁定技，摸牌阶段，你摸牌数量为你已损失的体力值+2，手牌上限始终+2
/// </summary>
public class JueJing : TriggerSkill
{
    protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        var args = eventArgs as AdjustmentEventArgs;
        args.AdjustmentAmount += 2;
    }

    public JueJing()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            Run,
            TriggerCondition.OwnerIsSource
        ) { IsAutoNotify = false };
        var trigger2 = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return p.LostHealth > 0; },
            (p, e, a) => { p[Player.DealAdjustment] += (p.LostHealth); },
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger2);
        Triggers.Add(GameEvent.PlayerHandCardCapacityAdjustment, trigger);
        IsEnforced = true;
    }

}
