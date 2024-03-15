using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Basic.Cards;


public class ShunShouQianYang : ShunChai
{
    protected override string ResultDeckName => "ShunShouChoice";

    protected override string ChoicePrompt => "ShunShou";

    protected override DeckPlace ShunChaiDest(Player source, Player dest)
    {
        return new DeckPlace(source, DeckType.Hand);
    }

    protected override bool ShunChaiAdditionalCheck(Player source, Player dest, ICard card)
    {
        var args = new AdjustmentEventArgs
        {
            Source = source,
            Targets = [dest],
            Card = card,
            AdjustmentAmount = 0
        };
        Game.CurrentGame.Emit(GameEvent.CardRangeModifier, args);
        if (Game.CurrentGame.DistanceTo(source, dest) > 1 + args.AdjustmentAmount)
        {
            return false;
        }
        return true;
    }
}
