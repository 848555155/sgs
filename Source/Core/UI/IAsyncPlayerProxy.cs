﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI;

public delegate void CardUsageAnsweredEventHandler(ISkill skill, List<Card> cards, List<Player> players);
public delegate void CardChoiceAnsweredEventHandler(List<List<Card>> cards);
public delegate void MultipleChoiceAnsweredEventHandler(int answer);
public interface IAsyncPlayerProxy
{
    Player HostPlayer { get; set; }
    bool IsPlayable { get; }
    void AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, int timeOutSeconds);
    void AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, int timeOutSeconds, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback);
    void AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, int timeOutSeconds);
    event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;
    event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;
    event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;
    void Freeze();
}
