﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Battle.Cards;


public class BingLiangCunDuan : DelayedTool
{
    public override void Activate(Player p, Card c)
    {
        while (true)
        {
            GameEventArgs args = new GameEventArgs();
            args.Source = null;
            args.Targets = new List<Player>() { p };
            args.Card = c;
            args.ReadonlyCard = new ReadOnlyCard(c);
            try
            {
                Game.CurrentGame.Emit(GameEvent.CardUsageBeforeEffected, args);
            }
            catch (TriggerResultException e)
            {
                Trace.Assert(e.Status == TriggerResult.End);
                break;
            }
            ReadOnlyCard result = Game.CurrentGame.Judge(p, null, c, (judgeResultCard) => { return judgeResultCard.Suit != SuitType.Club; });
            if (result.Suit != SuitType.Club)
            {
                Game.CurrentGame.PhasesSkipped.Add(TurnPhase.Draw);
            }
            break;
        }
        CardsMovement move = new CardsMovement();
        move.Cards = new List<Card>();
        move.Cards.Add(c);
        move.To = new DeckPlace(null, DeckType.Discard);
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
        // @todo: LooseVerify has not been defined.
        if (targets != null && targets.Count != 0 &&
            (targets.Count > 1 || DelayedToolConflicting(targets[0]) || targets[0] == source))
        {
            return VerifierResult.Fail;
        }
        if (targets == null || targets.Count == 0)
        {
            return VerifierResult.Partial;
        }
        Player dest = targets[0];
        var args = new AdjustmentEventArgs();
        args.Source = source;
        args.Targets = new List<Player>() { dest };
        args.Card = card;
        args.AdjustmentAmount = 0;
        Game.CurrentGame.Emit(GameEvent.CardRangeModifier, args);
        if (Game.CurrentGame.DistanceTo(source, dest) > 1 + args.AdjustmentAmount)
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }
}
