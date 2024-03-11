using Sanguosha.Core.Players;
using System.Diagnostics;

namespace Sanguosha.Core.Cards;

public class ReadOnlyCard(ICard card) : ICard
{
    public int Id { get; protected set; } = card is Card c ? c.Id : -1;
    public Player Owner { get; } = card.Owner;

    private readonly DeckPlace place = card.Place;
    public DeckPlace Place { get => place; set => Trace.Assert(false); }
    private readonly int rank = card.Rank;
    public int Rank { get => rank; set => Trace.Assert(false); }
    private readonly CardHandler type = card.Type;
    public CardHandler Type { get => type; set => Trace.Assert(false); }
    private readonly SuitType suit = card.Suit;
    public SuitType Suit { get => suit; set => Trace.Assert(false); }

    public SuitColorType SuitColor { get; } = card.SuitColor;

    public Dictionary<CardAttribute, int> Attributes { get; } = new Dictionary<CardAttribute, int>(card.Attributes ?? []);

    public int this[CardAttribute key]
    {
        get => !Attributes.TryGetValue(key, out int value) ? 0 : value;
        set
        {
            if (!Attributes.TryGetValue(key, out int v))
            {
                Attributes.Add(key, value);
            }
            else if (v == value)
            {
                return;
            }
            Attributes[key] = value;
        }
    }
}
