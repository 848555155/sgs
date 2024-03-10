﻿using System.Collections.Generic;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Fire.Skills;

/// <summary>
/// 七星-分发启始手牌时，共发你十一张牌，选四张作为手牌，其余的面朝下置于一旁，称为“星”。每当你于摸牌阶段摸牌后，可用任意数量的手牌等量替换这些“星”。
/// </summary>
public class QiXing : TriggerSkill
{
    private class QiXingVerifier : CardsAndTargetsVerifier
    {
        public QiXingVerifier(int QiXingCount)
        {
            MaxPlayers = 0;
            MinPlayers = 0;
            MaxCards = QiXingCount;
            MinCards = QiXingCount;
            Helper.NoCardReveal = true;
            Helper.ExtraTimeOutSeconds = 15;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand;
        }

    }

    private void GameStart(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        ISkill skill;
        List<Card> cards;
        List<Player> players;
        if (!Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QiXing", 7), new QiXingVerifier(7), out skill, out cards, out players))
        {
            cards = new List<Card>();
            cards.AddRange(Game.CurrentGame.Decks[Owner, DeckType.Hand].GetRange(0, 7));
        }
        CardsMovement move = new CardsMovement();
        move.Cards = new List<Card>(cards);
        move.To = new DeckPlace(Owner, QiXingDeck);
        move.Helper.IsFakedMove = true;
        move.Helper.PrivateDeckHeroTag = HeroTag;
        Game.CurrentGame.MoveCards(move);
    }

    private void AfterDraw(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        ISkill skill;
        List<Card> cards;
        List<Player> players;
        int qxCount = Game.CurrentGame.Decks[Owner, QiXingDeck].Count;
        // hack the cards to owner's hand. do not trigger anything
        CardsMovement move = new CardsMovement();
        move.Cards = new List<Card>(Game.CurrentGame.Decks[Owner, QiXingDeck]);
        move.To = new DeckPlace(Owner, DeckType.Hand);
        move.Helper.IsFakedMove = true;
        move.Helper.PrivateDeckHeroTag = HeroTag;
        Game.CurrentGame.MoveCards(move);
        if (!Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QiXing", qxCount), new QiXingVerifier(qxCount), out skill, out cards, out players))
        {
            cards = new List<Card>();
            cards.AddRange(Game.CurrentGame.Decks[Owner, DeckType.Hand].GetRange(0, qxCount));
        }
        move.Cards = new List<Card>(cards);
        move.To = new DeckPlace(Owner, QiXingDeck);
        move.Helper.IsFakedMove = true;
        move.Helper.PrivateDeckHeroTag = HeroTag;
        Game.CurrentGame.MoveCards(move);
    }

    public static PrivateDeckType QiXingDeck = new PrivateDeckType("QiXing");

    public QiXing()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            GameStart,
            TriggerCondition.OwnerIsSource
        ) { AskForConfirmation = false };
        var trigger2 = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return Game.CurrentGame.Decks[Owner, QiXingDeck].Count > 0; },
            AfterDraw,
            TriggerCondition.OwnerIsSource
        );
        var trigger3 = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { p[Player.DealAdjustment] += 7; },
            TriggerCondition.OwnerIsSource
        ) { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PlayerGameStartAction, trigger);
        Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.Draw], trigger2);
        Triggers.Add(GameEvent.StartGameDeal, trigger3);
        IsAutoInvoked = false;
        DeckCleanup.Add(QiXingDeck);
    }
}
