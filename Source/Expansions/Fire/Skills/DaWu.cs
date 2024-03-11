﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Fire.Skills;

/// <summary>
/// 大雾-回合结束阶段开始时，你可以弃掉X张“星”，指定X名角色，直到你的下回合开始，防止他们受到的除雷电伤害外的所有伤害。
/// </summary>
public class DaWu : TriggerSkill
{
    private class DaWuVerifier : CardsAndTargetsVerifier
    {
        public DaWuVerifier(int qxCount)
        {
            MaxPlayers = qxCount;
            MinPlayers = 1;
            MaxCards = qxCount;
            MinCards = 1;
            Helper.OtherDecksUsed.Add(QiXing.QiXingDeck);
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            int cp, cc;
            if (players == null) cp = 0; else cp = players.Count;
            if (cards == null) cc = 0; else cc = cards.Count;
            if (cp != cc) return null;
            return true;
        }
        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == QiXing.QiXingDeck;
        }

    }

    private List<Player> dawuTargets;
    public static readonly PlayerAttribute DaWuMark = PlayerAttribute.Register("DaWu", false, true);

    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        ISkill skill;
        List<Card> cards;
        List<Player> players;
        int qxCount = Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck].Count;
        if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("DaWu"), new DaWuVerifier(qxCount), out skill, out cards, out players))
        {
            NotifySkillUse(players);
            foreach (var mark in players)
            {
                mark[DaWuMark] = 1;
            }
            dawuTargets = players;
            Game.CurrentGame.HandleCardDiscard(null, cards);
            Trigger tri = new DaWuProtect();
            Game.CurrentGame.RegisterTrigger(GameEvent.DamageComputingStarted, tri);
            Game.CurrentGame.RegisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], new DawuRemoval(Owner, tri, this));
        }
    }

    private class DawuRemoval : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (!qixingOwner.IsDead)
            {
                if (eventArgs.Source != qixingOwner)
                {
                    return;
                }
                foreach (var mark in skill.dawuTargets) { mark[DaWuMark] = 0; }
                skill.dawuTargets.Clear();
            }
            Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], this);
            Game.CurrentGame.UnregisterTrigger(GameEvent.DamageComputingStarted, dawuProtect);
        }

        private readonly Player qixingOwner;
        private readonly Trigger dawuProtect;
        private readonly DaWu skill;
        public DawuRemoval(Player p, Trigger trigger, DaWu dawu)
        {
            qixingOwner = p;
            dawuProtect = trigger;
            skill = dawu;
        }
    }

    private class DaWuProtect : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as DamageEventArgs;
            if (args.Element == DamageElement.Lightning || eventArgs.Targets[0][DaWuMark] == 0)
            {
                return;
            }
            throw new TriggerResultException(TriggerResult.End);
        }
    }

    private class DaWuOnDeath : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Targets[0] != Owner) return;
            foreach (Player target in skill.dawuTargets)
            {
                target[DaWuMark] = 0;
            }
            Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerIsDead, this);
        }

        private readonly DaWu skill;
        public DaWuOnDeath(Player p, DaWu dawu)
        {
            Owner = p;
            skill = dawu;
        }
    }

    public DaWu()
    {
        dawuTargets = new List<Player>();
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck].Count > 0; },
            Run,
            TriggerCondition.OwnerIsSource
        )
        { IsAutoNotify = false };
        Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);

        var trigger2 = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { Game.CurrentGame.RegisterTrigger(GameEvent.PlayerIsDead, new DaWuOnDeath(p, this)); },
            TriggerCondition.OwnerIsSource
        )
        { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PlayerGameStartAction, trigger2);

        IsAutoInvoked = null;
    }
}
