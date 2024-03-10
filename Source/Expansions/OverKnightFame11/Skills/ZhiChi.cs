﻿using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame11.Skills;

/// <summary>
/// 智迟-锁定技，你的回合外，每当你受到一次伤害后，【杀】或非延时类锦囊牌对你无效，直到回合结束。
/// </summary>
public class ZhiChi : TriggerSkill
{
    public class ZhiChiProtect : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Targets[0] != Owner ||
                !(eventArgs.ReadonlyCard.Type.IsCardCategory(CardCategory.ImmediateTool) || eventArgs.ReadonlyCard.Type is Sha))
                return;
            throw new TriggerResultException(TriggerResult.End);
        }
        public ZhiChiProtect(Player p)
        {
            Owner = p;
        }
    }

    public class ZhiChiRemoval : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Source != Owner)
                return;
            skillOwner[ZhiChiStatus] = 0;
            Game.CurrentGame.UnregisterTrigger(GameEvent.CardUsageTargetValidating, protectTrigger);
            Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
        }

        private readonly Player skillOwner;
        private readonly Trigger protectTrigger;
        public ZhiChiRemoval(Player triggerOwner, Player skillOwner, Trigger trigger)
        {
            this.skillOwner = skillOwner;
            Owner = triggerOwner;
            protectTrigger = trigger;
        }
    }
    public static readonly PlayerAttribute ZhiChiStatus = PlayerAttribute.Register("ZhiChi", false, false, true);

    public ZhiChi()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => {return Game.CurrentGame.PhasesOwner != Owner;},
            (p, e, a) =>
            {
                Owner[ZhiChiStatus] = 1;
                Trigger tri = new ZhiChiProtect(Owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.CardUsageTargetValidating, tri);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new ZhiChiRemoval(Game.CurrentGame.CurrentPlayer, Owner, tri));
            },
            TriggerCondition.OwnerIsTarget
        );
        Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
        IsEnforced = true;
    }
}
