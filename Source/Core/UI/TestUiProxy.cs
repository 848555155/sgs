using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using System.Text.RegularExpressions;

namespace Sanguosha.Core.UI;

public class TestUiProxy : IPlayerProxy
{
    public void Freeze()
    {
    }

    public Player HostPlayer { get; set; }

    private StreamReader logFile;
    private string line;

    protected bool LoadTestScript(string fn)
    {
        try
        {
            logFile = new StreamReader(fn);
            if (!NextEntry())
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    protected bool NextEntry()
    {
        try
        {
            line = logFile.ReadLine();
        }
        catch (Exception)
        {
            line = null;
            return false;
        }
        if (line == null)
        {
            return false;
        }
        return true;
    }

    public bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
    {
        line = "U JiJiang 1: 2 3";
        Match m = Regex.Match(line, @"U\s(?<skill>[A-Za-z]*)(?<cards>(\s\d+)*):(?<players>(\s\d+)*)");
        skill = null;
        cards = null;
        players = null;
        if (m.Success)
        {
            if (m.Groups["skill"].Success)
            {
                foreach (var s in HostPlayer.Skills)
                {
                    if (s is CardTransformSkill)
                    {

                    }
                    if (s is ActiveSkill)
                    {
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, out int answer)
    {
        throw new NotImplementedException();
    }
    public int TimeOutSeconds { get; set; }


    public bool AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer, AdditionalCardChoiceOptions helper = null, CardChoiceRearrangeCallback callback = null)
    {
        throw new NotImplementedException();
    }


    public bool IsPlayable { get; set; }
}
