﻿using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

/// <summary>
/// 陷嗣 - 回合开始阶段开始时，你可以将至多两名角色的各一张牌移动至你的武将牌上，称为“逆”。每当其他角色需要对你使用一张【杀】时，该角色可以弃置你的一张“逆”，视为对你使用一张【杀】。
/// </summary>
public class XianSi : TriggerSkill
{
    private class XianSiVerifier : CardsAndTargetsVerifier
    {
        public XianSiVerifier()
        {
            MinCards = 0;
            MaxCards = 0;
            MinPlayers = 1;
            MaxPlayers = 2;
        }
        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }
        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player.HandCards().Count + player.Equipments().Count + player.DelayedTools().Count > 0;
        }
    }

    private void GetTheirCards(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        ISkill skill;
        List<Card> cards;
        List<Player> players;
        if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("XianSi"), new XianSiVerifier(), out skill, out cards, out players))
        {
            Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, players);
            NotifySkillUse(players);
            StagingDeckType XianSiTempDeck = new StagingDeckType("XianSiTemp");
            CardsMovement move = new CardsMovement();
            move.Helper.IsFakedMove = true;
            foreach (Player p in players)
            {
                if (p.HandCards().Count + p.Equipments().Count + p.DelayedTools().Count == 0) continue;
                List<List<Card>> answer;
                var deckplaces = new List<DeckPlace>() { new DeckPlace(p, DeckType.Hand), new DeckPlace(p, DeckType.Equipment), new DeckPlace(p, DeckType.DelayedTools) };
                if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("XianSi", p), deckplaces,
                    new List<string>() { "XianSi" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(true), out answer))
                {
                    answer = new List<List<Card>>();
                    answer.Add(Game.CurrentGame.PickDefaultCardsFrom(deckplaces));
                }
                move.Cards = answer[0];
                move.To = new DeckPlace(p, XianSiTempDeck);
                Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelays.None);
                Game.CurrentGame.PlayerLostCard(p, answer[0]);
            }
            move.Cards.Clear();
            move.Helper.IsFakedMove = false;
            move.To = new DeckPlace(Owner, NiDeck);
            foreach (Player p in players)
            {
                move.Cards.AddRange(Game.CurrentGame.Decks[p, XianSiTempDeck]);
            }
            Game.CurrentGame.SyncImmutableCardsAll(move.Cards);
            cards = new List<Card>(move.Cards);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.NotificationProxy.NotifyActionComplete();
            throw new TriggerResultException(TriggerResult.End);
        }
    }

    public static PrivateDeckType NiDeck = new PrivateDeckType("Ni", false);
    
    public XianSi()
    {
        LinkedSkill = new XianSiDistributor();
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            GetTheirCards,
            TriggerCondition.OwnerIsSource
        ) { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
        IsAutoInvoked = null;
    }

    public class XianSiGivenSkill : CardTransformSkill, IRulerGivenSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, List<Player> targets, out CompositeCard card, bool isPlay)
        {
            card = new CompositeCard();
            card.Type = new RegularSha();
            Trace.Assert(cards != null && targets != null);
            if (cards == null || targets == null) return VerifierResult.Fail;
            if (isPlay) return VerifierResult.Fail;
            if (Game.CurrentGame.Decks[Master, NiDeck].Count <= 1) return VerifierResult.Fail;
            if (cards.Any(cd => cd.Place.Player != master || cd.Place.DeckType != NiDeck)) return VerifierResult.Fail;
            if (cards.Count < 2) return VerifierResult.Partial;
            if (cards.Count > 2) return VerifierResult.Fail;
            card.Subcards = new List<Card>(cards);
            if (targets.Count == 0) return VerifierResult.Success;
            if (targets.Contains(Master)) return VerifierResult.Success;
            return VerifierResult.Fail;
        }

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets, bool isPlay)
        {
            Game.CurrentGame.HandleCardDiscard(master, new List<Card>(card.Subcards));
            card.Subcards = new List<Card>();
            return true;
        }

        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() { new Sha() }; }
        }
        public XianSiGivenSkill()
        {
        }

        private Player master;

        public Player Master
        {
            get { return master; }
            set { master = value; Helper.OtherGlobalCardDeckUsed.Clear(); Helper.OtherGlobalCardDeckUsed.Add(new DeckPlace(value, NiDeck), null); }
        }
    }

    public class XianSiDistributor : RulerGivenSkillContainerSkill
    {
        public XianSiDistributor()
            : base(new XianSiGivenSkill(), null, true)
        {
        }
    }
}
