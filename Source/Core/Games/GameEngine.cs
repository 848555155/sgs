using Sanguosha.Core.Cards;
using System.Diagnostics;
using System.Reflection;

namespace Sanguosha.Core.Games;

public class GameEngine
{
    public static IList<Card> CardSet { get; set; } = [];

    public static Dictionary<string, Expansion> Expansions { get; set; } = [];

    public static int Serialize(CardHandler handler) => idOfCardHandler[handler.Name];

    public static CardHandler DeserializeCardHandler(int id) => cardHandlers[id].Clone() as CardHandler;

    // Used to serialize/deserialize card handlers.
    private static readonly Dictionary<int, CardHandler> cardHandlers = [];
    private static readonly Dictionary<string, int> idOfCardHandler = [];

    public static void LoadExpansion(string name, Expansion expansion)
    {
        int newId = cardHandlers.Count;
        Expansions.Add(name, expansion);
        foreach (var card in expansion.CardSet)
        {
            card.Id = CardSet.Count;
            CardSet.Add(card);
            string typeName = card.Type.Name;
            if (!idOfCardHandler.ContainsKey(typeName))
            {
                idOfCardHandler.Add(typeName, newId);
                cardHandlers.Add(newId, card.Type);
                newId++;
            }
        }
    }

    public static void LoadExpansions(string folderPath)
    {
        List<string> packags = ["Assassin", "Basic", "Battle", "Fire", "Hills", "OverKnightFame11", "OverKnightFame11", "OverKnightFame11", "PK1v1", "SP", "StarSP", "Wind", "Woods"];
        // should not load all assembly because bug of sqlclient
        Trace.TraceInformation("LOADING CARDSETS FROM : " + folderPath);
        var files = (from f in Directory.GetFiles(folderPath) where f.EndsWith(".dll") select f)
            .Where(f => packags.Any(p => f.EndsWith(p + ".dll")))
            .OrderBy(
                    (a) =>
                    {
                        int idx = Properties.Settings.Default.LoadSequence.IndexOf(Path.GetFileNameWithoutExtension(a).ToLower());
                        return idx < 0 ? int.MaxValue : idx;
                    });
        foreach (var file in files)
        {
            try
            {
                foreach (var exp in Assembly
                    .LoadFrom(Path.GetFullPath(file))
                    .GetTypes()
                    .Where(type => type.IsSubclassOf(typeof(Expansion)))
                    .Select(Activator.CreateInstance)
                    .Cast<Expansion>())
                {
                    if (Expansions.ContainsKey(exp.GetType().FullName) && !Expansions.ContainsValue(exp))
                    {
                        Trace.TraceWarning("Cannot load two different expansions with same name: {0}.", exp.GetType().FullName);
                    }
                    else
                    {
                        LoadExpansion(exp.GetType().FullName, exp);
                    }
                }
            }
            catch (BadImageFormatException)
            {
                continue;
            }
        }
    }
}
