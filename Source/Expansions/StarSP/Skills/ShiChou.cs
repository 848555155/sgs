﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.StarSP.Skills;

/// <summary>
/// 誓仇–主公技，限定技，回合开始时，你可以交给一名蜀势力的其他角色两张牌，若如此做，直到该角色进入濒死状态前，当你受到伤害时，将该伤害转移给该角色，然后该角色摸等同于转移的伤害数值的牌。
/// </summary>
public class ShiChou : TriggerSkill
{
    private class ShiChouVerifier : CardsAndTargetsVerifier
    {
        public ShiChouVerifier()
        {
            MaxCards = 2;
            MinCards = 2;
            MaxPlayers = 1;
            MinPlayers = 1;
            Discarding = false;
        }
        protected override bool VerifyPlayer(Player source, Player player)
        {
            return source != player && player.Allegiance == Allegiance.Shu && source[ShiChouSource[player]] == 0;
        }
    }

    private class ShiChouProtect : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (!eventArgs.Targets.Contains(Owner)) return;
            DamageEventArgs damageArgs = eventArgs as DamageEventArgs;
            ReadOnlyCard rCard = new ReadOnlyCard(damageArgs.ReadonlyCard);
            rCard[ShiChouDamage] = 1;
            target[ShiChouTarget[Owner]]++;
            Game.CurrentGame.DoDamage(damageArgs.Source, target, Owner, damageArgs.Magnitude, damageArgs.Element, damageArgs.Card, rCard);
            throw new TriggerResultException(TriggerResult.End);
        }

        private readonly Player target;
        public ShiChouProtect(Player source, Player target)
        {
            Owner = source;
            this.target = target;
        }
    }

    private class ShiChouDrawCards : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (Owner[ShiChouTarget[source]] == 0 || !eventArgs.Targets.Contains(Owner) || eventArgs.ReadonlyCard[ShiChouDamage] == 0)
            {
                return;
            }
            Owner[ShiChouTarget[source]]--;
            Game.CurrentGame.DrawCards(Owner, (eventArgs as DamageEventArgs).Magnitude);
        }

        private readonly Player source;
        public ShiChouDrawCards(Player target, Player ShiChouSource)
        {
            Owner = target;
            source = ShiChouSource;
        }
    }

    private class ShiChouRemoval : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (!eventArgs.Targets.Contains(Owner)) return;
            Owner[ShiChouSource[source]] = 0;
            Owner[ShiChouStatus] = 0;
            Game.CurrentGame.UnregisterTrigger(GameEvent.DamageInflicted, protectTrigger);
            Game.CurrentGame.UnregisterTrigger(GameEvent.DamageComputingFinished, drawCardsTrigger);
            Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerIsAboutToDie, this);
        }

        private readonly Player source;
        private readonly Trigger protectTrigger;
        private readonly Trigger drawCardsTrigger;
        public ShiChouRemoval(Player target, Player source, Trigger protect, Trigger drawCards)
        {
            Owner = target;
            this.source = source;
            protectTrigger = protect;
            drawCardsTrigger = drawCards;
            Priority = int.MaxValue;
        }
    }

    private bool CanTriggerShiChou(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        return Owner[ShiChouUsed] == 0 && Game.CurrentGame.AlivePlayers.Any(p => { return p != Owner && p.Allegiance == Allegiance.Shu; });
    }

    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        ISkill skill;
        List<Card> cards;
        List<Player> players;
        if (Owner.AskForCardUsage(new CardUsagePrompt("ShiChou", this), new ShiChouVerifier(), out skill, out cards, out players))
        {
            NotifySkillUse(players);
            Owner[ShiChouUsed] = 1;
            players[0][ShiChouSource[Owner]] = 1;
            Game.CurrentGame.HandleCardTransferToHand(Owner, players[0], cards);
            players[0][ShiChouStatus] = 1;
            Trigger tri1 = new ShiChouProtect(Owner, players[0]);
            Trigger tri2 = new ShiChouDrawCards(players[0], Owner);
            Trigger tri3 = new ShiChouRemoval(players[0], Owner, tri1, tri2);
            Game.CurrentGame.RegisterTrigger(GameEvent.DamageInflicted, tri1);
            Game.CurrentGame.RegisterTrigger(GameEvent.DamageComputingFinished, tri2);
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerIsAboutToDie, tri3);
        }
    }

    public ShiChou()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
                  this,
                  CanTriggerShiChou,
                  Run,
                  TriggerCondition.OwnerIsSource
              )
        { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);

        IsRulerOnly = true;
        IsSingleUse = true;
    }

    private static readonly CardAttribute ShiChouDamage = CardAttribute.Register("ShiChouDamage");
    private static readonly PlayerAttribute ShiChouUsed = PlayerAttribute.Register("ShiChouUsed");
    private static readonly PlayerAttribute ShiChouSource = PlayerAttribute.Register("ShiChouSource");
    private static readonly PlayerAttribute ShiChouTarget = PlayerAttribute.Register("ShiChouTarget");
    private static readonly PlayerAttribute ShiChouStatus = PlayerAttribute.Register("ShiChou", false, false, true);
}
