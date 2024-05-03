using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Core.Skills;

public abstract class AutoVerifiedActiveSkill : ActiveSkill
{
    public override VerifierResult Validate(GameEventArgs arg)
    {
        Trace.Assert(Owner != null);
        if (Owner == null) return VerifierResult.Fail;
        return Verify(Owner, arg.Cards, arg.Targets);
    }

    protected int MinPlayers { get; set; } = 0;

    protected int MaxPlayers { get; set; } = int.MaxValue;

    protected int MinCards { get; set; } = 0;

    protected int MaxCards { get; set; } = int.MaxValue;

    /// <summary>
    /// Cards must pass "Game.CanDiscardCard" verification
    /// </summary>
    protected bool Discarding { get; set; } = false;

    protected abstract bool VerifyCard(Player source, Card card);

    protected abstract bool VerifyPlayer(Player source, Player player);

    protected virtual bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
    {
        return true;
    }

    public VerifierResult Verify(Player source, List<Card> cards, List<Player> players)
    {
        if (cards != null && cards.Count > MaxCards)
        {
            return VerifierResult.Fail;
        }
        if (cards != null && cards.Count > 0)
        {
            foreach (var c in cards)
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
            foreach (var p in players)
            {
                if (!VerifyPlayer(source, p))
                {
                    return VerifierResult.Fail;
                }
            }
        }
        bool? result = AdditionalVerify(source, cards, players);
        if (result == false)
        {
            return VerifierResult.Fail;
        }
        if (result == null)
        {
            return VerifierResult.Partial;
        }
        var count = players?.Count ?? 0;
        if (count < MinPlayers)
        {
            return VerifierResult.Partial;
        }
        count = cards?.Count ?? 0;
        if (count < MinCards)
        {
            return VerifierResult.Partial;
        }
        return VerifierResult.Success;
    }
}
