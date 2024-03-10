﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI;

public class GlobalDummyProxy : IGlobalUiProxy
{
    public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer)
    {
        cards = null;
        skill = null;
        players = null;
        respondingPlayer = null;
        return false;
    }

    public void AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, List<Player> players, out Dictionary<Player, int> aanswer)
    {
        aanswer = new Dictionary<Player, int>();
    }

    public void AskForHeroChoice(Dictionary<Player, List<Card>> restDraw, Dictionary<Player, List<Card>> heroSelection, int numberOfHeroes, ICardChoiceVerifier verifier)
    {
    }


    public void AskForMultipleCardUsage(Prompt prompt, ICardUsageVerifier verifier, List<Player> players, out Dictionary<Player, ISkill> askill, out Dictionary<Player, List<Card>> acards, out Dictionary<Player, List<Player>> aplayers)
    {
        acards = new Dictionary<Player, List<Card>>();
        aplayers = new Dictionary<Player, List<Player>>();
        askill = new Dictionary<Player, ISkill>();
    }

    public void Abort()
    {            
    }
}
