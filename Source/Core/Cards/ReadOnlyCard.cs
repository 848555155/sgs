using Sanguosha.Core.Players;
using System.Diagnostics;

namespace Sanguosha.Core.Cards;

public class ReadOnlyCard : ICard
{
    public ReadOnlyCard(ICard card)
    {
        type = card.Type;
        place = card.Place;
        rank = card.Rank;
        suit = card.Suit;
        Owner = card.Owner;
        SuitColor = card.SuitColor;
        if (card is Card c) Id = c.Id;
        else Id = -1;
        if (card.Attributes == null)
        {
            Attributes = [];
        }
        else
        {
            Attributes = new Dictionary<CardAttribute, int>(card.Attributes);
        }
    }
    public int Id { get; protected set; }
    public Player Owner { get; }

    private readonly DeckPlace place;
    public DeckPlace Place { get => place; set { Trace.Assert(false); } }
    private readonly int rank;
    public int Rank { get => rank; set { Trace.Assert(false); } }
    private readonly CardHandler type;
    public CardHandler Type { get => type; set { Trace.Assert(false); } }
    private readonly SuitType suit;
    public SuitType Suit { get => suit; set { Trace.Assert(false); } }

    public SuitColorType SuitColor { get; }

    public Dictionary<CardAttribute, int> Attributes { get; }

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
