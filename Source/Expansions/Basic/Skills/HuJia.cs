using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using Sanguosha.Expansions.Basic.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 护驾―主公技，当你需要使用或打出一张【闪】时，你可令其他魏势力角色打出一张【闪】(视为由你使用或打出)。
/// </summary>
public class HuJia : TriggerSkill
{
    private void CallOfShan(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        ICard result = null;
        List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
        toProcess.Remove(Owner);
        bool noAnswer = true;
        foreach (var player in toProcess)
        {
            if (player.Allegiance == Allegiance.Wei)
            {
                bool failToRespond = false;
                GameEventArgs args = new GameEventArgs
                {
                    Source = player,
                    Targets = eventArgs.Targets,
                    Card = new CompositeCard
                    {
                        Type = new Shan()
                    },
                    ReadonlyCard = eventArgs.ReadonlyCard
                };
                try
                {
                    Game.CurrentGame.Emit(GameEvent.PlayerRequireCard, args);
                }
                catch (TriggerResultException e)
                {
                    if (e.Status == TriggerResult.Success)
                    {
                        eventArgs.Skill = args.Skill;
                        eventArgs.Cards = new List<Card>(args.Cards);
                        throw new TriggerResultException(TriggerResult.Success);
                    }
                }
                while (true)
                {
                    IPlayerProxy ui = Game.CurrentGame.UiProxies[player];
                    SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Shan; }, false, new Shan());
                    ISkill skill;
                    List<Player> p;
                    List<Card> cards;
                    Game.CurrentGame.Emit(GameEvent.PlayerIsAboutToPlayCard, new PlayerIsAboutToUseOrPlayCardEventArgs() { Source = player, Verifier = v1 });
                    if (!ui.AskForCardUsage(new CardUsagePrompt("HuJia", Owner), v1, out skill, out cards, out p))
                    {
                        failToRespond = true;
                        break;
                    }
                    if (!Game.CurrentGame.CommitCardTransform(player, skill, cards, out result, eventArgs.Targets, true))
                    {
                        continue;
                    }
                    switch (result)
                    {
                        case CompositeCard:
                            eventArgs.Cards = new List<Card>((result as CompositeCard).Subcards);
                            eventArgs.Skill = new CardWrapper(Owner, new Shan());
                            break;
                        default:
                            eventArgs.Cards = [result as Card];
                            eventArgs.Skill = null;
                            break;
                    }
                    noAnswer = false;
                    Trace.TraceInformation("Player {0} Responded HuJia with SHAN, ", player.Id);
                    break;
                }
                if (failToRespond)
                {
                    continue;
                }
                break;
            }
        }

        if (noAnswer)
        {
            return;
        }

        Trace.Assert(result != null);
        eventArgs.Cards = new List<Card>();
        if (result is CompositeCard)
        {
            eventArgs.Cards.AddRange(((CompositeCard)result).Subcards);
        }
        else
        {
            Trace.Assert(result is Card);
            eventArgs.Cards.Add((Card)result);
        }
        throw new TriggerResultException(TriggerResult.Success);
    }

    private bool CanHuJia(Player p, GameEvent e, GameEventArgs a)
    {
        if (!((a.Card is CompositeCard) && ((a.Card as CompositeCard).Type is Shan)))
        {
            return false;
        }
        return Game.CurrentGame.AlivePlayers.Any(weiHero => weiHero != p && weiHero.Allegiance == Allegiance.Wei);
    }

    public HuJia()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            CanHuJia,
            CallOfShan,
            TriggerCondition.OwnerIsSource
        )
        { Type = TriggerType.Skill };
        Triggers.Add(GameEvent.PlayerRequireCard, trigger);
        IsAutoInvoked = false;
        IsRulerOnly = true;
    }

}
