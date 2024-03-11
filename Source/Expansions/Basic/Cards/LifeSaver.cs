﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards;

public class LifeSaver : CardHandler
{
    public override CardCategory Category
    {
        get { return CardCategory.Basic; }
    }

    protected override void Process(Core.Players.Player source, Core.Players.Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public override Core.UI.VerifierResult Verify(Core.Players.Player source, ICard card, List<Core.Players.Player> targets, bool isLooseVerify)
    {
        throw new NotImplementedException();
    }
}

public class LifeSaverVerifier : CardUsageVerifier
{
    public Player DyingPlayer { get; set; }
    public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        ICard card;
        if (skill != null)
        {
            if (skill is CardTransformSkill)
            {
                CompositeCard c;
                var result = (skill as CardTransformSkill).TryTransform(cards, null, out c);
                if (result != VerifierResult.Success)
                {
                    return result;
                }
                card = c;
            }
            else if (skill is SaveLifeSkill)
            {
                GameEventArgs arg = new GameEventArgs();
                arg.Source = skill.Owner;
                arg.Targets = players;
                arg.Cards = cards;
                return (skill as SaveLifeSkill).Validate(arg);
            }
            else return VerifierResult.Fail;
        }
        else
        {
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            card = cards[0];
        }
        if (!(card.Type is LifeSaver))
        {
            return VerifierResult.Fail;
        }
        return card.Type.Verify(source, skill, cards, new List<Player>());
    }

    public override IList<CardHandler> AcceptableCardTypes
    {
        get { return new List<CardHandler>() { new LifeSaver() }; }
    }
}

public class PlayerDying : Trigger
{
    private bool _CanUseTaoToSaveOther(Player player)
    {
        return Game.CurrentGame.PlayerCanUseCard(player, new Card() { Place = new DeckPlace(player, DeckType.None), Type = new Tao() });
    }

    private bool _CanUseSaveLifeSkillToSaveOther(Player player)
    {
        return player.ActionableSkills.Any(sk => sk is SaveLifeSkill && ((SaveLifeSkill)sk).Validate(new GameEventArgs() { Source = player }) != VerifierResult.Fail);
    }

    public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
    {
        Player target = eventArgs.Targets[0];
        if (target.Health > 0) return;
        LifeSaverVerifier v = new LifeSaverVerifier();
        v.DyingPlayer = target;
        List<Player> toAsk = Game.CurrentGame.AlivePlayers;
        foreach (Player p in toAsk)
        {
            if (p.IsDead) continue;
            if (!_CanUseTaoToSaveOther(p) && !_CanUseSaveLifeSkillToSaveOther(p) && p != target) continue;
            while (!target.IsDead && target.Health <= 0)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                Game.CurrentGame.Emit(GameEvent.PlayerIsAboutToUseCard, new PlayerIsAboutToUseOrPlayCardEventArgs() { Source = p, Verifier = v });
                if (Game.CurrentGame.UiProxies[p].AskForCardUsage(new CardUsagePrompt("SaveALife", target, 1 - target.Health), v, out skill, out cards, out players))
                {
                    if (skill != null && skill is SaveLifeSkill)
                    {
                        GameEventArgs arg = new GameEventArgs();
                        arg.Source = p;
                        arg.Targets = players;
                        arg.Cards = cards;
                        ((SaveLifeSkill)skill).NotifyAndCommit(arg);
                    }
                    else
                    {
                        try
                        {
                            GameEventArgs args = new GameEventArgs();
                            args.Source = p;
                            args.Skill = skill;
                            args.Cards = cards;
                            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                        }
                        catch (TriggerResultException e)
                        {
                            Trace.Assert(e.Status == TriggerResult.Retry);
                            continue;
                        }
                    }
                }
                else break;
            }
        }
    }
}
