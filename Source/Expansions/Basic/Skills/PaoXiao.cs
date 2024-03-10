﻿using System.Collections.Generic;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 咆哮-出牌阶段，你可以使用任意数量的【杀】。
/// </summary>
public class PaoXiao : TriggerSkill
{
    protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
    {
        Trace.Assert(Owner != null && Owner.Hero != null);
        if (Owner == null || Owner.Hero == null) return 0;
        else if (Owner.Hero.Name == "XiahouBa" || (Owner.Hero2 != null && Owner.Hero2.Name == "XiahouBa")) return 1;
        return 0;
    }

    private class PaoXiaoTrigger : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ShaEventArgs args = (ShaEventArgs)eventArgs;
            Trace.Assert(args != null);
            if (args.Source != Owner)
            {
                return;
            }
            args.TargetApproval[0] = true;
        }
    }

    private class PaoXiaoAlwaysShaTrigger : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Source == Owner)
            {
                throw new TriggerResultException(TriggerResult.Success);
            }
        }
    }

    public PaoXiao()
    {
        Triggers.Add(Sha.PlayerShaTargetValidation, new PaoXiaoTrigger());
        Triggers.Add(Sha.PlayerNumberOfShaCheck, new PaoXiaoAlwaysShaTrigger());
        AutoNotifyPassiveSkillTrigger aooo = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return p[Sha.NumberOfShaUsed] >= 1 && (a.Card.Type is Sha); },
            (p, e, a) => { },
            TriggerCondition.OwnerIsSource) { AskForConfirmation = false };
        Triggers.Add(GameEvent.PlayerUsedCard, aooo);
        IsEnforced = true;
    }
}
