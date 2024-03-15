using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI;

public class SingleCardUsageVerifier(SingleCardUsageVerifier.CardMatcher m, bool isUseCard, CardHandler handler = null) : CardUsageVerifier
{
    public delegate bool CardMatcher(ICard card);

    public CardMatcher Match { get; set; } = m;

    private readonly IList<CardHandler> possibleMatch = handler != null ? ([handler]) : null;
    private readonly bool isUseCard = isUseCard;

    public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        if (players != null && players.Count != 0)
        {
            return VerifierResult.Fail;
        }
        if (skill != null)
        {
            if (skill is not CardTransformSkill)
            {
                return VerifierResult.Fail;
            }
            CardTransformSkill s = (CardTransformSkill)skill;
            VerifierResult r = s.TryTransform(cards, players, out var card, !isUseCard);
            if (r != VerifierResult.Success)
            {
                return r;
            }
            if (!Match(card))
            {
                return VerifierResult.Fail;
            }
            if (isUseCard)
            {
                if (!Game.CurrentGame.PlayerCanUseCard(source, card))
                {
                    return VerifierResult.Fail;
                }
            }
            else
            {
                if (!Game.CurrentGame.PlayerCanPlayCard(source, card))
                {
                    return VerifierResult.Fail;
                }
            }
            return VerifierResult.Success;
        }
        if (cards != null && cards.Count > 1)
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
        if (!Match(cards[0]))
        {
            return VerifierResult.Fail;
        }
        if (isUseCard)
        {
            if (!Game.CurrentGame.PlayerCanUseCard(source, cards[0]))
            {
                return VerifierResult.Fail;
            }
        }
        else
        {
            if (!Game.CurrentGame.PlayerCanPlayCard(source, cards[0]))
            {
                return VerifierResult.Fail;
            }
        }
        return VerifierResult.Success;
    }


    public override IList<CardHandler> AcceptableCardTypes => possibleMatch;
}
