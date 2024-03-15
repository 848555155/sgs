using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Basic.Cards;


public class BaGuaZhen : Armor
{

    public class BaGuaZhenSkill : ArmorTriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ParentEquipment.InUse = true;
            ReadOnlyCard c = Game.CurrentGame.Judge(Owner, null, new Card() { Type = new BaGuaZhen() }, (judgeResultCard) => { return judgeResultCard.SuitColor == SuitColorType.Red; });
            ParentEquipment.InUse = false;
            if (c.SuitColor == SuitColorType.Red)
            {
                eventArgs.Cards = [];
                eventArgs.Skill = new CardWrapper(Owner, new Shan(), false);
                var log = new ActionLog
                {
                    Source = Owner,
                    SkillAction = this,
                    GameAction = GameAction.None
                };
                Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                throw new TriggerResultException(TriggerResult.Success);
            }
        }
        public BaGuaZhenSkill()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => a.Card.Type is Shan && ArmorIsValid(Owner, a.Targets[0], a.ReadonlyCard),
                Run,
                TriggerCondition.OwnerIsSource
            )
            { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerRequireCard, trigger);
        }
    }

    public BaGuaZhen()
    {
        EquipmentSkill = new BaGuaZhenSkill() { ParentEquipment = this };
    }

}
