using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards;

public class CompositeCard : ICard
{
    public CompositeCard()
    {
        Subcards = [];
        attributes = null;
    }

    public CompositeCard(List<Card> cards)
    {
        Subcards = cards;
        attributes = null;
    }

    public List<Card> Subcards { get; set; }

    public virtual Player Owner { get; set; }

    public virtual DeckPlace Place
    {
        get
        {
            var places = Subcards.Select(card => card.Place).Distinct();
            if (places.Count() == 1)
            {
                return places.First();
            }
            else
            {
                return new DeckPlace(null, DeckType.None);
            }
        }
        set
        {
            foreach (var card in Subcards)
            {
                card.Place = value;
            }
        }
    }


    /// <summary>
    /// 如果只改变牌或由牌变成的标记的名称（一张牌当另一张牌使用，如龙胆，火计），则花色和点数默认为不改变
    /// 将多张牌当一张牌使用或打出时，视为无花色无点数
    /// </summary>
    public virtual int Rank
    {
        get => Subcards.Count == 1 ? Subcards[0].Rank : 0;
        set => throw new NotSupportedException("Cannot set rank of a composite card.");
    }

    public virtual SuitType Suit
    {
        get => Subcards.Count == 1 ? Subcards[0].Suit : SuitType.None;
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Suit color of a composite card is the uniform color of its Subcards, or None if
    /// no uniform color exists.
    /// </summary>
    /// <remarks>
    /// 将多张牌当一张牌使用或打出时，除非这些牌的颜色均相同（视为相应颜色但无花色），否则视为无色且无花色。
    /// </remarks>
    public virtual SuitColorType SuitColor
    {
        get
        {
            if (Subcards is null) 
                return SuitColorType.None;
            var colors = Subcards.Select(card => card.SuitColor);
            if (colors is not null) colors = colors.Distinct();
            else colors = [];
            if (colors.Count() == 1)
            {
                return colors.First();
            }
            else
            {
                return SuitColorType.None;
            }
        }
    }

    public virtual CardHandler Type { get; set; }

    protected Dictionary<CardAttribute, int> attributes;

    public Dictionary<CardAttribute, int> Attributes { get; set; }

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

}