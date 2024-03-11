﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards;


public class Tao : LifeSaver
{
    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        if (readonlyCard[EatOneGetAnotherFreeCoupon] == 1)
        {
            Game.CurrentGame.RecoverHealth(source, dest, 2);
        }
        else
        {
            Game.CurrentGame.RecoverHealth(source, dest, 1);
        }
    }

    public override List<Player> ActualTargets(Player source, List<Player> targets, ICard card)
    {
        if (targets.Count > 0)
        {
            return new List<Player>(targets);
        }

        if (Game.CurrentGame.DyingPlayers.Count > 0)
        {
            return new List<Player>() { Game.CurrentGame.DyingPlayers.First() };
        }
        else
        {
            return new List<Player>() { source };
        }
    }

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        Trace.Assert(targets != null);
        if (targets == null) return VerifierResult.Fail;

        if ((!isLooseVerify && targets.Count > 0) ||
            ActualTargets(source, targets, card).Any(p => p.Health >= p.MaxHealth))
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }

    public override CardCategory Category
    {
        get { return CardCategory.Basic; }
    }
    public static readonly CardAttribute EatOneGetAnotherFreeCoupon = CardAttribute.Register("EatOneGetAnotherFreeCoupon");
}
