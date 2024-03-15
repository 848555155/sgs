using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI;

public class CardsAndTargetsVerifier : ICardUsageVerifier
{
    protected int MinPlayers { get; set; }

    protected int MaxPlayers { get; set; }

    protected int MinCards { get; set; }

    protected int MaxCards { get; set; }

    /// <summary>
    /// Cards must pass "Game.CanDiscardCard" verification
    /// </summary>
    protected bool Discarding { get; set; }

    public CardsAndTargetsVerifier()
    {
        MinPlayers = 0;
        MaxPlayers = int.MaxValue;
        MinCards = 0;
        MaxCards = int.MaxValue;
        Discarding = false;
        Helper = new UiHelper();
    }

    protected virtual bool VerifyCard(Player source, Card card)
    {
        return true;
    }

    protected virtual bool VerifyPlayer(Player source, Player player)
    {
        return true;
    }

    protected virtual bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
    {
        return true;
    }

    public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        if (players != null && players.Any(pl => pl.IsDead)) return VerifierResult.Fail;
        if (skill != null)
        {
            return VerifierResult.Fail;
        }
        if (cards != null && cards.Count > MaxCards)
        {
            return VerifierResult.Fail;
        }
        if (cards != null && cards.Count > 0)
        {
            foreach (Card c in cards)
            {
                if (Discarding && !Game.CurrentGame.PlayerCanDiscardCard(source, c))
                {
                    return VerifierResult.Fail;
                }
                if (!VerifyCard(source, c))
                {
                    return VerifierResult.Fail;
                }
            }
        }
        if (players != null && players.Count > MaxPlayers)
        {
            return VerifierResult.Fail;
        }
        if (players != null && players.Count > 0)
        {
            foreach (Player p in players)
            {
                if (!VerifyPlayer(source, p))
                {
                    return VerifierResult.Fail;
                }
            }
        }
        var ret = AdditionalVerify(source, cards, players);
        if (ret == false)
        {
            return VerifierResult.Fail;
        }
        if (ret == null)
        {
            return VerifierResult.Partial;
        }
        int count = players == null ? 0 : players.Count;
        if (count < MinPlayers)
        {
            return VerifierResult.Partial;
        }
        count = cards == null ? 0 : cards.Count;
        if (count < MinCards)
        {
            return VerifierResult.Partial;
        }
        return VerifierResult.Success;
    }

    public virtual IList<CardHandler> AcceptableCardTypes
    {
        get { return null; }
    }

    public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        return FastVerify(source, skill, cards, players);
    }

    public UiHelper Helper { get; protected set; }
}
