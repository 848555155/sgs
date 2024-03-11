using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

public class DanShou : TriggerSkill
{
    protected void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        Game.CurrentGame.DrawCards(owner, 1);
        throw new EndOfTurnException();
    }

    public DanShou()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            Run,
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.AfterDamageCaused, trigger);
        IsAutoInvoked = false;
    }
}
