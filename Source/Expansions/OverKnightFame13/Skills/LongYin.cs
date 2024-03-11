﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

public class LongYin : TriggerSkill
{
    private class LongYinVerifier : CardsAndTargetsVerifier
    {
        public LongYinVerifier()
        {
            MinCards = 1;
            MaxCards = 1;
            MaxPlayers = 0;
            MinPlayers = 0;
        }
        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand;
        }
    }
    public LongYin()
    {
        var trigger = new AutoNotifyUsagePassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                return a.Card.Type is Sha && Game.CurrentGame.CurrentPhase == TurnPhase.Play;
            },
            (p, e, a, c, pls) =>
            {
                Game.CurrentGame.HandleCardDiscard(p, c);
                if (a.Card.SuitColor == SuitColorType.Red) Game.CurrentGame.DrawCards(p, 1);
                a.Source[Sha.NumberOfShaUsed]--;
            },
            TriggerCondition.Global,
            new LongYinVerifier()
        )
        { IsAutoNotify = false };

        Triggers.Add(GameEvent.PlayerUsedCard, trigger);
        IsAutoInvoked = null;
    }
}
