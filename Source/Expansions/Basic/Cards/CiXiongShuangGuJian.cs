﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Basic.Cards;


public class CiXiongShuangGuJian : Weapon
{
    public CiXiongShuangGuJian()
    {
        EquipmentSkill = new CiXiongShuangGuJianSkill() { ParentEquipment = this };
    }


    public class CiXiongShuangGuJianSkill : TriggerSkill, IEquipmentSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            SingleCardDiscardVerifier v = new SingleCardDiscardVerifier();
            if (!Game.CurrentGame.UiProxies[eventArgs.Targets[0]].AskForCardUsage(new CardUsagePrompt("CiXiong2", eventArgs.Source), v, out skill, out cards, out players))
            {
                Game.CurrentGame.DrawCards(eventArgs.Source, 1);
            }
            else
            {
                Game.CurrentGame.HandleCardDiscard(eventArgs.Targets[0], cards);
            }
        }
        public CiXiongShuangGuJianSkill()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return (a.ReadonlyCard.Type is Sha) && ((a.Targets[0].IsFemale && a.Source.IsMale) ||
                        (a.Targets[0].IsMale && a.Source.IsFemale));
                },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
        }

        public Equipment ParentEquipment { get; set; }
    }

    public override int AttackRange
    {
        get { return 2; }
    }

    protected override void RegisterWeaponTriggers(Player p)
    {
        return;
    }

    protected override void UnregisterWeaponTriggers(Player p)
    {
        return;
    }

}
