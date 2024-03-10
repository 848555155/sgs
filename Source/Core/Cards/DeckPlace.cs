using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards;

public class DeckPlace(Player player, DeckType deckType)
{
    public Player Player { get; set; } = player;

    public DeckType DeckType { get; set; } = deckType;

    public override string ToString() => $"Player {Player.Id}, {DeckType}";

    public override bool Equals(object obj)
    {
        var dp = obj as DeckPlace;
        if (dp == null)
            return false;
        return Player == dp.Player && DeckType == dp.DeckType;
    }

    public override int GetHashCode()
    {
        return ((Player == null) ? 0 : Player.GetHashCode()) + ((DeckType == null) ? 0 : DeckType.GetHashCode());
    }

    public static bool operator ==(DeckPlace a, DeckPlace b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if ((a is null) || (b is null))
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(DeckPlace a, DeckPlace b)
    {
        return !(a == b);
    }
}
