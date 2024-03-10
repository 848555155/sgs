using Sanguosha.Core.Players;

namespace Sanguosha.UI.Controls;


public enum GameResult
{
    Win,
    Lose,
    Draw,
}

public class GameResultViewModel : ViewModelBase
{
    public Player Player { get; set; }
    public string UserName { get; set; }
    public string GainedExperience { get; set; }
    public string GainedTechPoints { get; set; }
    public GameResult Result { get; set; }
    public string Comments { get; set; }
}
