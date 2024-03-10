﻿using System.Collections.Generic;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Pk1v1.Cards;

namespace Sanguosha.Expansions.Pk1v1.Skills;

/// <summary>
/// 虎威-你登场时，你可视为对对手使用一张【水淹七军】。
/// </summary>
public class HuWei : TriggerSkill
{
    private class HuWeiVerifier : CardUsageVerifier
    {
        public override VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            return FastVerify(source, skill, cards, players);
        }

        public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (players == null || players.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (players[0] == source)
            {
                return VerifierResult.Fail;
            }
            return new ShuiYanQiJun().Verify(source, null, players, false);
        }

        public override IList<CardHandler> AcceptableCardTypes
        {
            get { return null; }
        }

        public HuWeiVerifier()
        {
        }
    }

    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        ISkill skill;
        List<Card> cards;
        List<Player> players;
        if (Owner.AskForCardUsage(new CardUsagePrompt("HuWei"), new HuWeiVerifier(), out skill, out cards, out players))
        {
            NotifySkillUse();
            GameEventArgs args = new GameEventArgs();
            args.Source = Owner;
            args.Targets = players;
            args.Skill = new CardWrapper(Owner, new ShuiYanQiJun(), false);
            args.Cards = new List<Card>();
            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
        }
    }

    public HuWei()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                return true;
            },
            Run,
            TriggerCondition.OwnerIsSource
        ) { AskForConfirmation = false, IsAutoNotify = false };


        Triggers.Add(GameEvent.HeroDebut, trigger);
        IsAutoInvoked = null;
    }
}
