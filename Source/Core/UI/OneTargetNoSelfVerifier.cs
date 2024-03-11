﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI;

public class OneTargetNoSelfVerifier : ICardUsageVerifier
{

    public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        if (skill != null || (cards != null && cards.Count != 0))
        {
            return VerifierResult.Fail;
        }
        if (players == null || players.Count == 0)
        {
            return VerifierResult.Partial;
        }
        if (players.Count > 1)
        {
            return VerifierResult.Fail;
        }
        if (players[0] == source)
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }

    public IList<CardHandler> AcceptableCardTypes
    {
        get { return null; }
    }

    public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        return FastVerify(source, skill, cards, players);
    }

    public UiHelper Helper
    {
        get { return new UiHelper(); }
    }
}
