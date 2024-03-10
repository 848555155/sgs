using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI;

public class ConsoleUiProxy : IPlayerProxy
{
    public Player HostPlayer { get; set; }

#pragma warning disable CA1033 // 接口方法应可由子类型调用
    bool IPlayerProxy.AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
#pragma warning restore CA1033 // 接口方法应可由子类型调用
    {
        Player p = HostPlayer;
        Console.Write("I AM PLAYER {0}({1}): ", p.Id, p.Hero.Name);
        cards = new List<Card>();
        VerifierResult r = verifier.FastVerify(HostPlayer, null, null, null);
        skill = null;
        Console.Write("Ask for card usage, {0}: ", prompt);
        int i = 0;
        foreach (ICard card in Game.CurrentGame.Decks[p, DeckType.Hand])
        {
            Console.Write(" Card {0} {1}{2}{3}, ", i, card.Suit, card.Rank, card.Type.Name);
            i++;
        }
    again:
        Console.Write("Card id, -1 to trigger");
        string ids = Console.ReadLine();
        if (ids == null) { Thread.Sleep(200); cards = null; players = null; return false; }
        int id = int.Parse(ids);
        if (id < -1)
        {
            cards = null;
            players = null;
        }
        else
        {
            if (id == -1)
            {
                Console.Write("Skill ID:");
                ids = Console.ReadLine();
                id = int.Parse(ids);
                if (id >= HostPlayer.Skills.Count || ((!(HostPlayer.Skills[id] is ActiveSkill)) && (!(HostPlayer.Skills[id] is CardTransformSkill))))
                {
                    goto again;
                }
                skill = HostPlayer.Skills[id];
                while (true)
                {
                    Console.Write("Card id, -1 to end");
                    ids = Console.ReadLine();
                    id = int.Parse(ids);
                    if (id < 0)
                    {
                        break;
                    }
                    if (id >= Game.CurrentGame.Decks[p, DeckType.Hand].Count)
                    {
                        continue;
                    }
                    cards.Add(Game.CurrentGame.Decks[p, DeckType.Hand][id]);
                }
            }
            else
            {
                if (id < Game.CurrentGame.Decks[p, DeckType.Hand].Count)
                {
                    cards.Add(Game.CurrentGame.Decks[p, DeckType.Hand][id]);
                }
            }
            players = null;
            r = verifier.FastVerify(HostPlayer, skill, cards, players);
            if (r == VerifierResult.Success)
            {
                return true;
            }
            if (r == VerifierResult.Fail)
            {
                Console.Write("Failed check, again? 1 yes 0 no");
                ids = Console.ReadLine();
                id = int.Parse(ids);
                if (id == 1)
                {
                    goto again;
                }
            }
        }
        {
            players = new List<Player>();
            while (true)
            {
                Console.WriteLine("");
                Console.Write("Target player: -1 to end");
                ids = Console.ReadLine();
                id = int.Parse(ids);
                if (id < 0)
                {
                    break;
                }
                if (id > Game.CurrentGame.Players.Count)
                {
                    Console.WriteLine("out of range");
                }
                else
                {
                    players.Add(Game.CurrentGame.Players[id]);
                }
                r = verifier.FastVerify(HostPlayer, skill, cards, players);
                if (r == VerifierResult.Partial)
                {
                    Console.WriteLine("Require more");
                }
                if (r == VerifierResult.Fail)
                {
                    Console.WriteLine("Failed check");
                    players.Remove(Game.CurrentGame.Players[id]);
                }
            }
            r = verifier.FastVerify(HostPlayer, skill, cards, players);
            if (r == VerifierResult.Success)
            {
                return true;
            }
            if (r == VerifierResult.Fail)
            {
                Console.Write("Failed check, again? 1 yes 0 no");
                ids = Console.ReadLine();
                id = int.Parse(ids);
                if (id == 1)
                {
                    goto again;
                }
            }
            players = null;
            skill = null;
            cards = null;
            return false;

        }
    }

#pragma warning disable CA1033 // 接口方法应可由子类型调用
    bool IPlayerProxy.AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions options, CardChoiceRearrangeCallback callback)
#pragma warning restore CA1033 // 接口方法应可由子类型调用
    {
        answer = null;
        return false;
    }


    public bool AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, out int answer)
    {
        /*
        Player p = hostPlayer;
        Console.Write("I AM PLAYER {0}: ", p.Id);
        Console.Write(prompt + ":");
        foreach (string s in questions)
        {
            Console.Write(" " + s + ", ");
        }
        Console.Write("Choose:");
        string ids = Console.ReadLine();
        int id = int.Parse(ids);
        if (id > questions.Count || id < 0)
        {
            answer = 0;
            return false;
        }
        answer = id;
        return true;*/
        answer = 0;
        return false;
    }
    public int TimeOutSeconds { get; set; }


    public void Freeze()
    {
    }


    public bool IsPlayable
    {
        get;
        set;
    }
}
