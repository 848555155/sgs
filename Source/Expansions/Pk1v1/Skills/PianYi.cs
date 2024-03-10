using Sanguosha.Core.Triggers;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Pk1v1.Skills;

internal class PianYi : TriggerSkill
{

    public PianYi()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                return Game.CurrentGame.CurrentPlayer != p;
            },
            (p, e, a) => { throw new EndOfTurnException(); },
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.HeroDebut, trigger);
        IsEnforced = true;
    }
}
