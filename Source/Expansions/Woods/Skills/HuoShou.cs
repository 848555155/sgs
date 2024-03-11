﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Woods.Skills;

/// <summary>
/// 祸首-锁定技，【南蛮入侵】对你无效；锁定技，每当其他角色使用【南蛮入侵】指定目标后，你成为此【南蛮入侵】造成伤害的来源。
/// </summary>
public class HuoShou : TriggerSkill
{
    private class HuoShouTrigger : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.ReadonlyCard[HuoShouChangeSource] == 1)
            {
                if (!Owner.IsDead) eventArgs.Source = Owner;
                else eventArgs.Source = null;
            }
        }

        public HuoShouTrigger(Player p)
        {
            Owner = p;
        }
    }

    public override Player Owner
    {
        get
        {
            return base.Owner;
        }
        set
        {
            if (base.Owner == value)
            {
                return;
            }
            base.Owner = value;
            if (base.Owner != null)
            {
                if (_huoshouChangeSource != null)
                {
                    Game.CurrentGame.UnregisterTrigger(GameEvent.DamageSourceConfirmed, _huoshouChangeSource);
                }
                _huoshouChangeSource = new HuoShouTrigger(base.Owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.DamageSourceConfirmed, _huoshouChangeSource);
            }
        }
    }

    private Trigger _huoshouChangeSource;
    public HuoShou()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.ReadonlyCard.Type is NanManRuQin; },
            (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
            TriggerCondition.OwnerIsTarget
        )
        { Type = TriggerType.Skill };
        var trigger2 = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.ReadonlyCard.Type is NanManRuQin && a.Source != p; },
            (p, e, a) => { a.ReadonlyCard[HuoShouChangeSource] = 1; },
            TriggerCondition.Global
        );
        Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
        Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger2);
        IsEnforced = true;
    }

    private static readonly CardAttribute HuoShouChangeSource = CardAttribute.Register("HuoShouChangeSource");
}
