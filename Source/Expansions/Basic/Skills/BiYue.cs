using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 闭月-回合结束阶段开始时，你可以摸一张牌。
/// </summary>
public class BiYue : TriggerSkill
{
    public BiYue()
    {
        Trigger trigger = new AutoNotifyPassiveSkillTrigger
        (
            this,
            (p, e, a) => { Game.CurrentGame.DrawCards(p, 1); },
            TriggerCondition.OwnerIsSource
        );

        Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
        IsAutoInvoked = true;
    }
}
