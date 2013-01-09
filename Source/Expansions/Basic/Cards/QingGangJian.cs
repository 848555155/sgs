﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class QingGangJian : Weapon
    {
        public QingGangJian()
        {
            EquipmentSkill = new QingGangJianSkill() { ParentEquipment = this };
        }

        
        public class QingGangJianSkill : TriggerSkill, IEquipmentSkill
        {
            public Equipment ParentEquipment { get; set; }
            protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                foreach (var target in eventArgs.Targets)
                {
                    eventArgs.ReadonlyCard[Armor.IgnorePlayerArmor[target]] = 1;
                }
            }

            public QingGangJianSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return a.ReadonlyCard != null && (a.ReadonlyCard.Type is Sha);
                    },
                    Run,
                    TriggerCondition.OwnerIsSource
                );
                Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
                IsEnforced = true;
            }
        }

        public override int AttackRange
        {
            get { return 2; }
        }


        protected override void RegisterWeaponTriggers(Player p)
        {
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
        }
    }
}
