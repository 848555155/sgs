using System.Diagnostics;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Cards;


public abstract class DelayedTool : CardHandler
{
    public override CardCategory Category => CardCategory.DelayedTool;

    protected void AttachTo(Player source, Player target, ICard c)
    {
        var m = new CardsMovement();
        if (c is CompositeCard compositeCard)
        {
            m.Cards = new List<Card>(compositeCard.Subcards);
        }
        else
        {
            m.Cards = [];
            var card = (Card)c;
            Trace.Assert(card != null);
            m.Cards.Add(card);
        }
        m.To = new DeckPlace(target, DeckType.DelayedTools);
        var cards = new List<Card>(m.Cards);
        Game.CurrentGame.MoveCards(m);
        Game.CurrentGame.PlayerLostCard(source, cards);
    }

    public virtual bool DelayedToolConflicting(Player p)
    {
        foreach (var c in Game.CurrentGame.Decks[p, DeckType.DelayedTools])
        {
            if (GetType().IsAssignableFrom(c.Type.GetType()))
            {
                return true;
            }
        }
        return false;
    }

    public abstract void Activate(Player p, Card c);

}
