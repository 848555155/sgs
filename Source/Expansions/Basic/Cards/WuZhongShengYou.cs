using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Basic.Cards;


public class WuZhongShengYou : CardHandler
{
    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        Game.CurrentGame.DrawCards(dest, 2);
    }

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        if (!isLooseVerify && targets != null && targets.Count >= 1)
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }

    public override List<Player> ActualTargets(Player source, List<Player> targets, ICard card)
    {
        if (targets.Count > 0)
        {
            return new List<Player>(targets);
        }

        return new List<Player>() { source };
    }

    public override CardCategory Category
    {
        get { return CardCategory.ImmediateTool; }
    }
}
