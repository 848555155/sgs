using Sanguosha.Core.Players;
using System.Diagnostics;

namespace Sanguosha.Core.Cards;

public class Card : ICard
{
    public bool IsUnknown => (Id < 0);

    public static readonly int UnknownCardId = -1;
    public static readonly int UnknownHeroId = -2;
    public static readonly int UnknownRoleId = -3;
    public static readonly int UnknownSPHeroId = -4;

    public bool RevealOnce { get; set; }

    public Card()
    {
        Suit = SuitType.None;
        Rank = 0;
        Type = null;
        RevealOnce = false;
        Attributes = null;
    }

    public Card(SuitType t, int r, CardHandler c)
    {
        Suit = t;
        Rank = r;
        Type = c;
        Attributes = null;
    }

    public void CopyFrom(Card c)
    {
        Suit = c.Suit;
        Rank = c.Rank;
        Type = (CardHandler)c.Type.Clone();
        Trace.Assert(Type != null);
        RevealOnce = false;
        Place = c.Place;
        Id = c.Id;
        Attributes = c.Attributes;
    }

    public Card(Card c)
    {
        Suit = c.Suit;
        Rank = c.Rank;
        Type = c.Type;
        RevealOnce = false;
        Place = c.Place;
        Id = c.Id;
        Attributes = c.Attributes;
    }

    public Card(ICard c)
    {
        Suit = c.Suit;
        Rank = c.Rank;
        Type = c.Type;
        RevealOnce = false;
        Place = c.Place;
        Id = -1;
        Attributes = c.Attributes;
    }

    public DeckPlace HistoryPlace2 { get; set; }
    public DeckPlace HistoryPlace1 { get; set; }
    public DeckPlace Place { get; set; }

    /// <summary>
    /// Computational owner of the card.
    /// </summary>
    /// <remarks>
    /// 每名角色的牌包括其手牌、装备区里的牌和判定牌。该角色的判定牌和其判定区里的牌都不为任何角色所拥有。
    /// </remarks>
    public Player Owner => Place.DeckType == DeckType.Hand ||
                Place.DeckType == DeckType.Equipment || Place.DeckType is StagingDeckType
                ? Place.Player
                : null;

    public int Id { get; set; }

    public SuitType Suit { get; set; }

    public SuitColorType SuitColor => Suit switch
    {
        SuitType.Heart or SuitType.Diamond => SuitColorType.Red,
        SuitType.Spade or SuitType.Club => SuitColorType.Black,
        _ => SuitColorType.None
    };

    public int Rank { get; set; }

    public CardHandler Type { get; set; }

    public Dictionary<CardAttribute, int> Attributes { get; private set; }

    public int this[CardAttribute key]
    {
        get
        {
            Attributes ??= [];
            return !Attributes.TryGetValue(key, out int value) ? 0 : value;
        }
        set
        {
            Attributes ??= [];
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

    public static readonly CardAttribute IsLastHandCard = CardAttribute.Register(nameof(IsLastHandCard));

    #region UI Related

    public UI.ActionLog Log { get; set; } = new();

    public DeckPlace PlaceOverride { get; set; }
    #endregion
}
