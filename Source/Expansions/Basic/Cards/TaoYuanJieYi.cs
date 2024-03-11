﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Basic.Cards;


public class TaoYuanJieYi : CardHandler
{
    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        if (dest.Health >= dest.MaxHealth)
        {
            return;
        }
        Game.CurrentGame.RecoverHealth(source, dest, 1);
    }

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        if (targets != null && targets.Count >= 1)
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }

    public override CardCategory Category
    {
        get { return CardCategory.ImmediateTool; }
    }

    public override List<Player> ActualTargets(Player source, List<Player> dests, ICard card)
    {
        var targets = new List<Player>(Game.CurrentGame.AlivePlayers);
        var backup = new List<Player>(targets);
        foreach (var t in backup)
        {
            if (!Game.CurrentGame.PlayerCanBeTargeted(source, new List<Player>() { t }, card))
            {
                targets.Remove(t);
            }
            if (t.Health >= t.MaxHealth)
            {
                targets.Remove(t);
            }
        }
        return targets;
    }
}
