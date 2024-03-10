using System.Collections.Generic;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Wind.Skills;

/// <summary>
/// 红颜-锁定技，你的黑桃牌均视为红桃牌。
/// </summary>
public class HongYan : EnforcedCardTransformSkill
{
    public HongYan()
    {
        Decks.Add(DeckType.Hand);
        Decks.Add(DeckType.JudgeResult);
    }

    protected override bool CardVerifier(ICard card)
    {
        return card.Suit == SuitType.Spade;
    }

    protected override void TransfromAction(Player Owner, ICard card)
    {
        card.Suit = SuitType.Heart;
        if (card.Place.DeckType == DeckType.JudgeResult)
        {
            NotifySkillUse();
            Game.CurrentGame.NotificationProxy.NotifyLogEvent(
                new LogEvent("EnforcedCardTransform", Owner, GameEngine.CardSet[(card as Card).Id], card),
                new List<Player> { Owner },
                true,
                false
            );
        }
    }
}
