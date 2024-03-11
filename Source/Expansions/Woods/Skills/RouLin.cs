﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Woods.Skills;

/// <summary>
/// 肉林-锁定技，你对女性角色、女性角色对你使用【杀】时，需连续使用两张【闪】才能抵消。
/// </summary>
public class RouLin : TriggerSkill
{
    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        if (Owner != eventArgs.Source)
        {
            foreach (var pl in eventArgs.Targets)
            {
                if (Owner == pl)
                {
                    eventArgs.ReadonlyCard[CardAttribute.TargetRequireTwoResponses[pl]] = 1;
                }
            }
        }
        else
        {
            foreach (var pl in eventArgs.Targets)
            {
                if (pl.IsFemale)
                {
                    eventArgs.ReadonlyCard[CardAttribute.TargetRequireTwoResponses[pl]] = 1;
                }
            }
        }
    }

    public RouLin()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return ((a.Targets.Contains(p) && a.Source.IsFemale) || (a.Source == p && a.Targets.Any(tar => tar.IsFemale))) && (a.ReadonlyCard.Type is Sha); },
            Run,
            TriggerCondition.Global
        );

        Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
        IsEnforced = true;
    }
}
