﻿using Sanguosha.Core.Triggers;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.OverKnightFame12.Skills;

/// <summary>
/// 宗室-锁定技，你的手牌上限+X（X为现存势力数）。
/// </summary>
public class ZongShi : TriggerSkill
{
    public ZongShi()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                var args = a as AdjustmentEventArgs;
                args.AdjustmentAmount += Game.CurrentGame.NumberOfAliveAllegiances;
            },
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.PlayerHandCardCapacityAdjustment, trigger);
        IsEnforced = true;
    }
}
