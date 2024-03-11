﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

/// <summary>
/// 称象-每当你受到一次伤害后，你可以展示所有手牌，若点数之和小于13，你摸一张牌。你可以重复此流程，直至你的所有手牌点数之和等于或大于13为止。
/// </summary>
public class ChengXiang : TriggerSkill
{
    private class DaXiangVerifier : ICardChoiceVerifier
    {
        public VerifierResult Verify(List<List<Card>> answer)
        {
            if (answer == null || answer.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (answer[0] == null || answer[0].Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (answer[0].Sum(c => c.Rank) >= 13) return VerifierResult.Fail;
            return VerifierResult.Success;
        }

        private readonly List<Card> cards;
        public DaXiangVerifier(List<Card> c)
        {
            cards = new List<Card>(c);
        }
        public UiHelper Helper
        {
            get { return new UiHelper() { ShowToAll = true }; }
        }
    }

    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        DeckType daXiangDeck = DeckType.Register("DaXiang");

        CardsMovement move = new CardsMovement();
        move.Cards = new List<Card>();
        for (int i = 0; i < 4; i++)
        {
            Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
            Card c = Game.CurrentGame.DrawCard();
            move.Cards.Add(c);
        }
        move.To = new DeckPlace(null, daXiangDeck);
        Game.CurrentGame.MoveCards(move);
        List<List<Card>> answer;
        if (Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("ChengXiang", Owner),
                new List<DeckPlace>() { new DeckPlace(null, daXiangDeck) },
                new List<string>() { "AcquiredCards" },
                new List<int>() { 4 },
                new DaXiangVerifier(Game.CurrentGame.Decks[null, daXiangDeck]),
                out answer,
                null,
                CardChoiceCallback.GenericCardChoiceCallback))
        {
            Game.CurrentGame.HandleCardTransferToHand(null, Owner, answer[0]);
        }

        foreach (var c in Game.CurrentGame.Decks[null, daXiangDeck])
        {
            c.Log.SkillAction = this;
            c.Log.GameAction = GameAction.PlaceIntoDiscard;
        }
        Game.CurrentGame.PlaceIntoDiscard(null, new List<Card>(Game.CurrentGame.Decks[null, daXiangDeck]));
    }

    public ChengXiang()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            Run,
            TriggerCondition.OwnerIsTarget
        );
        Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
    }
}