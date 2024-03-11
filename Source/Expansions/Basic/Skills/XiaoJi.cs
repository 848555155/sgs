using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 枭姬-当你失去装备区里的一张牌时，你可以摸两张牌。
/// </summary>
public class XiaoJi : TriggerSkill
{
    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        foreach (Card c in eventArgs.Cards)
        {
            if (c.HistoryPlace1.DeckType == DeckType.Equipment && c.HistoryPlace1.Player == Owner)
            {
                if (AskForSkillUse())
                {
                    NotifySkillUse(new List<Player>());
                    Game.CurrentGame.DrawCards(Owner, 2);
                }
            }
        }
    }

    public XiaoJi()
    {
        var trigger = new RelayTrigger(
            Run,
            TriggerCondition.OwnerIsSource
        )
        { Priority = SkillPriority.XiaoJi };
        Triggers.Add(GameEvent.CardsLost, trigger);
        IsAutoInvoked = true;
    }
}
