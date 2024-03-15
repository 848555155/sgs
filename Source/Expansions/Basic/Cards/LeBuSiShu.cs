using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards;


public class LeBuSiShu : DelayedTool
{
    public override void Activate(Player p, Card c)
    {
        while (true)
        {
            var args = new GameEventArgs
            {
                Source = null,
                Targets = [p],
                Card = c,
                ReadonlyCard = new ReadOnlyCard(c)
            };
            try
            {
                Game.CurrentGame.Emit(GameEvent.CardUsageBeforeEffected, args);
            }
            catch (TriggerResultException e)
            {
                Trace.Assert(e.Status == TriggerResult.End);
                break;
            }
            ReadOnlyCard result = Game.CurrentGame.Judge(p, null, c, (judgeResultCard) => { return judgeResultCard.Suit != SuitType.Heart; });
            if (result.Suit != SuitType.Heart)
            {
                Game.CurrentGame.PhasesSkipped.Add(TurnPhase.Play);
            }
            break;
        }
        var move = new CardsMovement
        {
            Cards = [c],
            To = new DeckPlace(null, DeckType.Discard)
        };
        move.Helper.IsFakedMove = true;
        Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelays.None);
    }

    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public override void Process(GameEventArgs handlerArgs)
    {
        var source = handlerArgs.Source;
        var dests = handlerArgs.Targets;
        var readonlyCard = handlerArgs.ReadonlyCard;
        var inResponseTo = handlerArgs.InResponseTo;
        var card = handlerArgs.Card;
        Trace.Assert(dests.Count == 1);
        AttachTo(source, dests[0], card);
    }

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        if (targets != null && targets.Count != 0 &&
            (targets.Count > 1 || DelayedToolConflicting(targets[0]) || targets[0] == source))
        {
            return VerifierResult.Fail;
        }
        if (targets == null || targets.Count == 0)
        {
            return VerifierResult.Partial;
        }
        return VerifierResult.Success;
    }
}
