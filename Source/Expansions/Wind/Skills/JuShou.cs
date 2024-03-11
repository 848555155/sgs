using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Wind.Skills;

/// <summary>
/// 据守-回合结束阶段开始时，你可以摸三张牌，然后将你的武将牌翻面。
/// </summary>
public class JuShou : TriggerSkill
{
    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        Game.CurrentGame.DrawCards(Owner, 3);
        Owner.IsImprisoned = !Owner.IsImprisoned;
    }

    public JuShou()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            Run,
            TriggerCondition.OwnerIsSource
        );

        Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
    }

}
