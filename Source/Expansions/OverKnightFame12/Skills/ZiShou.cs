﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    public class ZiShou : TriggerSkill
    {
        /// <summary>
        /// 自守-摸牌阶段，若你已受伤，你可以额外摸X张牌（X为你已损失的体力值），然后跳过你的出牌阶段。
        /// </summary>
        public ZiShou()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return p.Health < p.MaxHealth;
                },
                (p, e, a) =>
                {
                    p[Player.DealAdjustment] += p.MaxHealth - p.Health;
                    var theTrigger = new LeBuSiShu.LeBuSiShuTrigger() { Priority = int.MaxValue };
                    theTrigger.Owner = p;
                    Game.CurrentGame.RegisterTrigger(GameEvent.PhaseOutEvents[TurnPhase.Draw], theTrigger);
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = null;
        }
    }
}
