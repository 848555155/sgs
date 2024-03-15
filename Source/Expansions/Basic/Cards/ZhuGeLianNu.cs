﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Basic.Cards;


public class ZhuGeLianNu : Weapon
{
    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public ZhuGeLianNu()
    {
        EquipmentSkill = new ZhuGeLianNuSkill() { ParentEquipment = this };
    }

    private class ZhuGeLianNuSkill : TriggerSkill, IEquipmentSkill
    {
        public Equipment ParentEquipment { get; set; }
        public ZhuGeLianNuSkill()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => a.Source[Sha.NumberOfShaUsed] > 0 && a.Card.Type is Sha,
                (p, e, a) => { },
                TriggerCondition.OwnerIsSource
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { throw new TriggerResultException(TriggerResult.Success); },
                TriggerCondition.OwnerIsSource
            )
            { IsAutoNotify = false };
            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    ShaEventArgs args = (ShaEventArgs)a;
                    args.TargetApproval[0] = true;
                },
                TriggerCondition.OwnerIsSource
            )
            { IsAutoNotify = false, Priority = int.MaxValue, Type = TriggerType.Skill };
            Triggers.Add(GameEvent.PlayerUsedCard, trigger);
            Triggers.Add(Sha.PlayerNumberOfShaCheck, trigger2);
            Triggers.Add(Sha.PlayerShaTargetValidation, trigger3);
            IsEnforced = true;
        }
    }

    public override int AttackRange
    {
        get { return 1; }
    }

    protected override void RegisterWeaponTriggers(Player p)
    {
    }

    protected override void UnregisterWeaponTriggers(Player p)
    {
    }
}
