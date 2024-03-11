using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Basic.Cards;


public class ShunShouQianYang : ShunChai
{
    protected override string ResultDeckName
    {
        get { return "ShunShouChoice"; }
    }

    protected override string ChoicePrompt
    {
        get { return "ShunShou"; }
    }

    protected override DeckPlace ShunChaiDest(Player source, Player dest)
    {
        return new DeckPlace(source, DeckType.Hand);
    }

    protected override bool ShunChaiAdditionalCheck(Player source, Player dest, ICard card)
    {
        var args = new AdjustmentEventArgs();
        args.Source = source;
        args.Targets = new List<Player>() { dest };
        args.Card = card;
        args.AdjustmentAmount = 0;
        Game.CurrentGame.Emit(GameEvent.CardRangeModifier, args);
        if (Game.CurrentGame.DistanceTo(source, dest) > 1 + args.AdjustmentAmount)
        {
            return false;
        }
        return true;
    }
}
