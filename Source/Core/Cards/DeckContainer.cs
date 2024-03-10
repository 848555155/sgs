using Sanguosha.Core.Players;
using System.Diagnostics;

namespace Sanguosha.Core.Cards;

public class DeckContainer
{
    protected class GameAsPlayer : Player
    {
        private static GameAsPlayer instance;

        private GameAsPlayer() { }

        public static GameAsPlayer Instance
        {
            get
            {
                instance ??= new GameAsPlayer();
                return instance;
            }
        }
    }

    public List<Card> this[DeckType type]
    {
        get { return this[null, type]; }
        set { this[null, type] = value; }
    }

    public List<Card> this[Player player, DeckType type]
    {
        get
        {
            player ??= GameAsPlayer.Instance;
            if (!GameDecks.ContainsKey(player))
            {
                GameDecks[player] = [];
            }
            if (!GameDecks[player].ContainsKey(type))
            {
                GameDecks[player][type] = [];
            }
            return GameDecks[player][type];
        }

        set
        {
            if (player == null)
            {
                player = GameAsPlayer.Instance;
            }
            if (!GameDecks.ContainsKey(player))
            {
                GameDecks[player] = [];
            }
            if (!GameDecks[player].ContainsKey(type))
            {
                GameDecks[player][type] = value;
            }
        }
    }

    public List<Card> this[DeckPlace place]
    {
        get { return this[place.Player, place.DeckType]; }
        set { this[place.Player, place.DeckType] = value; }
    }

    protected Dictionary<Player, Dictionary<DeckType, List<Card>>> GameDecks { get; set; } = [];

    public List<DeckType> GetPlayerPrivateDecks(Player player)
    {
        List<DeckType> list = [];
        Trace.Assert(player != null);
        if (!GameDecks.Keys.Contains(player)) return list;
        var result = GameDecks[player]
            .Where(kvp => kvp.Key is PrivateDeckType && GameDecks[player][kvp.Key].Count > 0)
            .Select(kvp => kvp.Key);
        list.AddRange(result);
        return list;
    }

    public List<Card> GetPlayerPrivateCards(Player player)
    {
        var result = new List<Card>();
        foreach (var deckType in GetPlayerPrivateDecks(player))
        {
            result.AddRange(this[player, deckType]);
        }
        return result;
    }
}
