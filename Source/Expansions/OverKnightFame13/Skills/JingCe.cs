using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

/// <summary>
/// 精策-出牌阶段结束时，若你本回合使用的牌数量大于或等于你当前体力值，你可以摸两张牌。
/// </summary>
public class JingCe : TriggerSkill
{
    private int _count;
    public JingCe()
    {
        _count = 0;
        var cardUsedCount = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                _count++;
            },
            TriggerCondition.OwnerIsSource
        )
        { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PlayerUsedCard, cardUsedCount);

        var tagClear = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                _count = 0;
            },
            TriggerCondition.Global
        )
        { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PhaseBeforeStart, tagClear);

        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return _count >= p.Health; },
            (p, e, a) =>
            {
                Game.CurrentGame.DrawCards(p, 2);
            },
            TriggerCondition.Global
        );
        Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.Play], trigger);

        IsAutoInvoked = null;
    }
}
