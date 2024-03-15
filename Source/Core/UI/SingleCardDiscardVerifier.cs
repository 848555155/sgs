using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI;

public class SingleCardDiscardVerifier : CardUsageVerifier
{
    public delegate bool CardMatcher(ICard card);

    public CardMatcher Match { get; set; }

    private readonly IList<CardHandler> possibleMatch;

    public SingleCardDiscardVerifier(CardMatcher m = null, CardHandler handler = null)
    {
        Match = m;
        if (handler != null)
        {
            possibleMatch = [handler];
        }
        else
        {
            possibleMatch = null;
        }
    }

    public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        if (skill != null || (cards != null && cards.Count > 1) || (players != null && players.Count != 0))
        {
            return VerifierResult.Fail;
        }
        if (cards == null || cards.Count == 0)
        {
            return VerifierResult.Partial;
        }
        if (cards[0].Place.DeckType != DeckType.Hand)
        {
            return VerifierResult.Fail;
        }
        if (Match != null && !Match(cards[0]))
        {
            return VerifierResult.Fail;
        }
        if (!Game.CurrentGame.PlayerCanDiscardCard(source, cards[0]))
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }

    public override IList<CardHandler> AcceptableCardTypes => possibleMatch;
}
