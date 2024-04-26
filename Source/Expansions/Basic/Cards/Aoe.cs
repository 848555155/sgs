using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards;

public abstract class Aoe : CardHandler
{
    protected abstract string UsagePromptString { get; }

    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return RequiredCard.GetType().IsAssignableFrom(c.Type.GetType()); }, false, RequiredCard);
        List<Player> sourceList = [source];
        GameEventArgs args = new GameEventArgs
        {
            Source = dest,
            Targets = sourceList,
            Card = new CompositeCard()
        };
        args.Card.Type = RequiredCard;
        args.ReadonlyCard = readonlyCard;
        try
        {
            Game.CurrentGame.Emit(GameEvent.PlayerRequireCard, args);
        }
        catch (TriggerResultException e)
        {
            if (e.Status == TriggerResult.Success)
            {
                Game.CurrentGame.HandleCardPlay(dest, args.Skill, args.Cards, sourceList);
                return;
            }
        }
        while (true)
        {
            IPlayerProxy ui = Game.CurrentGame.UiProxies[dest];
            Game.CurrentGame.Emit(GameEvent.PlayerIsAboutToPlayCard, new PlayerIsAboutToUseOrPlayCardEventArgs() { Source = dest, Verifier = v1 });
            if (!ui.AskForCardUsage(new CardUsagePrompt(UsagePromptString, source),
                                                  v1, out var skill, out var cards, out var p))
            {
                Trace.TraceInformation("Player {0} Invalid answer", dest);
                Game.CurrentGame.DoDamage(source.IsDead ? null : source, dest, 1, DamageElement.None, card, readonlyCard);
            }
            else
            {
                if (!Game.CurrentGame.HandleCardPlay(dest, skill, cards, sourceList))
                {
                    continue;
                }
                Trace.TraceInformation("Player {0} Responded. ", dest.Id);
            }
            break;
        }
    }

    public abstract CardHandler RequiredCard
    {
        get;
        protected set;
    }

    public override List<Player> ActualTargets(Player source, List<Player> dests, ICard card)
    {
        var targets = new List<Player>(Game.CurrentGame.AlivePlayers);
        targets.Remove(source);
        var backup = new List<Player>(targets);
        foreach (var t in backup)
        {
            if (!Game.CurrentGame.PlayerCanBeTargeted(source, new List<Player>() { t }, card))
            {
                targets.Remove(t);
            }
        }
        return targets;
    }

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        if (targets != null && targets.Count >= 1)
        {
            return VerifierResult.Fail;
        }
        if (ActualTargets(source, targets, card).Count == 0) return VerifierResult.Fail;
        return VerifierResult.Success;
    }

    public override CardCategory Category => CardCategory.ImmediateTool;
}
