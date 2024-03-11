using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

public class AnJian : TriggerSkill
{
    public AnJian()
    {
        var trigger2 = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard.Type is Sha && (a as DamageEventArgs).Targets[0][Player.AttackRange] + 1 < Game.CurrentGame.DistanceTo((a as DamageEventArgs).Targets[0], p); },
            (p, e, a) => { (a as DamageEventArgs).Magnitude++; },
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.DamageElementConfirmed, trigger2);
        IsEnforced = true;
    }

}
