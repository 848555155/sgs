﻿using System.Collections.Generic;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 铁骑-当你使用【杀】指定了一名角色为目标后，你可以进行一次判定，若结果为红色，此【杀】不可被【闪】响应。
/// </summary>
public class TieJi : TriggerSkill
{
    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        Dictionary<Player, int> map = new Dictionary<Player, int>();
        foreach (var target in eventArgs.Targets)
        {
            if (!map.Keys.Contains(target)) map.Add(target, 0);
            if (AskForSkillUse())
            {
                NotifySkillUse(new List<Player>() { target });
                var card = Game.CurrentGame.Judge(Owner, this, null, (judgeResultCard) => { return judgeResultCard.SuitColor == SuitColorType.Red; });
                if (card.SuitColor == SuitColorType.Red)
                {
                    eventArgs.ReadonlyCard[ShaCancelling.CannotProvideShan[target]] |= (1 << map[target]);
                }
            }
            map[target]++;
        }
    }

    public TieJi()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.ReadonlyCard.Type is Sha; },
            Run,
            TriggerCondition.OwnerIsSource
        ) { AskForConfirmation = false, IsAutoNotify = false };

        Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
        IsAutoInvoked = false;
    }

}
