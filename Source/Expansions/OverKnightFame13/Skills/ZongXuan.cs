﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

public class ZongXuan : TriggerSkill
{
    protected void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        var args = eventArgs as DiscardCardEventArgs;
        List<Card> cardsToProcess = new List<Card>(eventArgs.Cards);
        foreach (Card c in cardsToProcess)
        {
            var prompt = new MultipleChoicePrompt("ZongXuan", c);
            int answer = 0;
            if (Owner.AskForMultipleChoice(prompt, Prompt.YesNoChoices, out answer) && answer == 1)
            {
                Game.CurrentGame.InsertBeforeDeal(owner, new List<Card>() { c });
                eventArgs.Cards.Remove(c);
            }
        }
    }

    public ZongXuan()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return (a as DiscardCardEventArgs).Source == p && (a as DiscardCardEventArgs).Reason == DiscardReason.Discard; },
            Run,
            TriggerCondition.Global
        )
        { IsAutoNotify = false, AskForConfirmation = false };
        Triggers.Add(GameEvent.CardsEnteringDiscardDeck, trigger);
        IsAutoInvoked = false;
    }
}
