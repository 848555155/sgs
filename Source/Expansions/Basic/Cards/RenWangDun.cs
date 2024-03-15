using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Basic.Cards;


public class RenWangDun : Armor
{

    public class RenWangDunSkill : ArmorTriggerSkill
    {
        public RenWangDunSkill()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => a.ReadonlyCard != null && (a.ReadonlyCard.Type is Sha) && a.ReadonlyCard.SuitColor == SuitColorType.Black && ArmorIsValid(Owner, a.Source, a.ReadonlyCard),
                (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                TriggerCondition.OwnerIsTarget
            );
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
            IsEnforced = true;
        }
    }

    public RenWangDun()
    {
        EquipmentSkill = new RenWangDunSkill() { ParentEquipment = this };
    }

}
