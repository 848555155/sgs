using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Basic.Cards;


public class GuoHeChaiQiao : ShunChai
{
    protected override string ResultDeckName => "GuoHeChoice";

    protected override string ChoicePrompt => "GuoHe";

    protected override DeckPlace ShunChaiDest(Player source, Player dest)
    {
        return new DeckPlace(null, DeckType.Discard);
    }

    protected override bool ShunChaiAdditionalCheck(Player source, Player dest, ICard card)
    {
        return true;
    }

}
