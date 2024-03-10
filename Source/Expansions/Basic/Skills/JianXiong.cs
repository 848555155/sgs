﻿using System.Collections.Generic;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 奸雄-每当你受到一次伤害后，你可以获得对你造成伤害的牌。
/// </summary>
public class JianXiong : TriggerSkill
{
    public JianXiong()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                Trace.Assert(a.Cards != null);
                return a.Cards.Count > 0;
            },
            (p, e, a) =>
            {
                Game.CurrentGame.HandleCardTransferToHand(null, Owner, new List<Card>(a.Cards));
            },
            TriggerCondition.OwnerIsTarget
        );
        Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
        IsAutoInvoked = false;
    }
}
