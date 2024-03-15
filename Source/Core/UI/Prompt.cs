namespace Sanguosha.Core.UI;


public class CardUsagePrompt(string key, params object[] args) : Prompt(CardUsagePromptsPrefix + key, args);

public class CardChoicePrompt(string key, params object[] args) : Prompt(CardChoicePromptsPrefix + key, args);

public class MultipleChoicePrompt(string key, params object[] args) : Prompt(MultipleChoicePromptPrefix + key, args);

public class OptionPrompt(string key, params object[] args) : Prompt(MultipleChoiceOptionPrefix + key, args);

public class LogEvent(string key, params object[] args) : Prompt(LogEventPrefix + key, args);

public class LogEventArg(string key, params object[] args) : Prompt(LogEventArgPrefix + key, args);

public class Prompt
{
    public Prompt()
    {
        _values = [];
    }

    private readonly List<object> _values;

    public Prompt(string resourceKey, params object[] args)
        : this()
    {
        ResourceKey = resourceKey;
        _values.AddRange(args);
    }

    public string ResourceKey { get; set; }

    public IList<object> Values => _values;

    #region Resource Converter Prefixes
    public static readonly string DirectOutputPrefix = "#";
    #endregion

    #region Card Usage Prompts
    public static readonly string CardUsagePromptsPrefix = "CardUsage.Prompt.";
    public static readonly string PlayingPhasePrompt = CardUsagePromptsPrefix + "Play";
    public static readonly string DiscardPhasePrompt = CardUsagePromptsPrefix + "Discard";
    #endregion

    #region Card Choice Prompts
    public static readonly string CardChoicePromptsPrefix = "CardChoice.Prompt.";
    #endregion

    #region Log Event
    public static readonly string LogEventPrefix = "LogEvent.";
    public static readonly string LogEventArgPrefix = "LogEvent.Arg.";
    public static readonly LogEventArg Success = new LogEventArg("Success");
    public static readonly LogEventArg Fail = new LogEventArg("Fail");
    #endregion

    #region Multiple Choice Constants
    public static readonly string MultipleChoicePromptPrefix = "MultiChoice.Prompt.";
    public static readonly string MultipleChoiceOptionPrefix = "MultiChoice.Choice.";
    public static readonly string NonPlaybleAppendix = ".Others";
    public static readonly string SkillUseYewNoPrompt = "SkillYesNo";
    public static readonly OptionPrompt YesChoice = new OptionPrompt("Yes");
    public static readonly OptionPrompt NoChoice = new OptionPrompt("No");
    public static readonly OptionPrompt HeartChoice = new OptionPrompt("Heart");
    public static readonly OptionPrompt SpadeChoice = new OptionPrompt("Spade");
    public static readonly OptionPrompt ClubChoice = new OptionPrompt("Club");
    public static readonly OptionPrompt DiamondChoice = new OptionPrompt("Diamond");
    public static readonly List<OptionPrompt> YesNoChoices = [NoChoice, YesChoice];
    public static readonly List<OptionPrompt> SuitChoices = [ClubChoice, SpadeChoice, HeartChoice, DiamondChoice];
    public static readonly List<OptionPrompt> RecoverOneHealthOrDrawOneCardOptions = [new OptionPrompt("RecoverHealth", 1), new OptionPrompt("DrawCards", 1)];
    public static readonly List<OptionPrompt> RecoverOneHealthOrDrawTwoCardsOptions = [new OptionPrompt("RecoverHealth", 1), new OptionPrompt("DrawCards", 2)];
    public static readonly List<OptionPrompt> AllegianceChoices = [new OptionPrompt("Qun"), new OptionPrompt("Shu"), new OptionPrompt("Wei"), new OptionPrompt("Wu")];
    #endregion
}
