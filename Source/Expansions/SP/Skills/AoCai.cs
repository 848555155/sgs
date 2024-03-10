﻿using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.SP.Skills;

public class AoCai : CardTransformSkill
{
    public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
    {
        card = new CompositeCard();
        card.Subcards = new List<Card>();

        if (Game.CurrentGame.CurrentPlayer == Owner)
        {
            return VerifierResult.Fail;
        }

        if (cards == null || cards.Count == 0)
        {
            return VerifierResult.Partial;
        }
        if (cards != null && cards.Count > 1)
        {
            return VerifierResult.Fail;
        }
        if (!(cards[0] == Game.CurrentGame.Decks[null, DeckType.Dealing][0] || cards[0] == Game.CurrentGame.Decks[null, DeckType.Dealing][1]))
        {
            return VerifierResult.Fail;
        }
        if (cards[0].Type.BaseCategory() != CardCategory.Basic) return VerifierResult.Fail;
        card.Type = cards[0].Type;
        card.Subcards.Add(cards[0]);
        return VerifierResult.Success;
    }

    public AoCai()
    {
        LinkedPassiveSkill = new AoCaiPassive();
        Helper.OtherGlobalCardDeckUsed.Add(new DeckPlace(null, DeckType.Dealing), 2);
    }

    public CardHandler AdditionalType { get; set; }

    public class AoCaiPassive : TriggerSkill
    {
        public AoCaiPassive()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    if (Game.CurrentGame.CurrentPlayer == Owner) return false;
                    var pa = a as PlayerIsAboutToUseOrPlayCardEventArgs;
                    Trace.Assert(pa != null && pa.Verifier != null);
                    if (pa == null || pa.Verifier == null || pa.Verifier.AcceptableCardTypes == null) return false;
                    return pa.Verifier.AcceptableCardTypes.Any(t => t.BaseCategory() == CardCategory.Basic);
                },
                (p, e, a) =>
                {
                    //ensure we have two cards in the dealing deck
                    List<Card> cards = new List<Card>();
                    cards.Add(Game.CurrentGame.PeekCard(0));
                    cards.Add(Game.CurrentGame.PeekCard(1));
                    Game.CurrentGame.SyncImmutableCards(p, cards);
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsAboutToPlayCard, trigger);
            Triggers.Add(GameEvent.PlayerIsAboutToUseCard, trigger);
        }
    }
}
