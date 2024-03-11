using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Triggers;

public class GameEventArgs
{
    public GameEventArgs()
    {
        Targets = new List<Player>();
        Cards = new List<Card>();
    }

    public void CopyFrom(GameEventArgs another)
    {
        Source = another.Source;
        Targets = new List<Player>(another.Targets);
        Cards = new List<Card>(another.Cards);
        Card = another.Card;
        ReadonlyCard = another.ReadonlyCard;
    }

    public Player Source { get; set; }

    public List<Player> Targets { get; set; }

    public List<Player> UiTargets { get; set; }

    public List<Card> Cards { get; set; }

    public ISkill Skill { get; set; }

    public ICard Card { get; set; }

    /// <summary>
    /// Gets/sets the game event(arg) that this game event(arg) is responding to.
    /// </summary>
    /// <remarks>
    /// 仅在闪用于抵消杀以及无懈可击用于抵消锦囊时被设置
    /// </remarks>
    public GameEventArgs InResponseTo { get; set; }

    public ReadOnlyCard ReadonlyCard { get; set; }
}

public class HealthChangedEventArgs : GameEventArgs
{
    public HealthChangedEventArgs() { }

    public HealthChangedEventArgs(DamageEventArgs args)
    {
        CopyFrom(args);
        Delta = -args.Magnitude;
    }

    /// <summary>
    /// Gets/sets the health change value.
    /// </summary>
    public int Delta
    {
        get;
        set;
    }
}

public class DamageEventArgs : GameEventArgs
{
    public Player OriginalTarget
    {
        get;
        set;
    }

    /// <summary>
    /// Gets/sets the magnitude of damage
    /// </summary>
    public int Magnitude
    {
        get;
        set;
    }

    public DamageElement Element
    {
        get;
        set;
    }

}

public class DiscardCardEventArgs : GameEventArgs
{
    public DiscardReason Reason { get; set; }
}

public class AdjustmentEventArgs : GameEventArgs
{
    public int AdjustmentAmount { get; set; }
    public int OriginalAmount { get; set; }
}

public class PinDianCompleteEventArgs : GameEventArgs
{
    public List<bool> CardsResult { get; set; }
    public bool? PinDianResult { get; set; }
}

public class SkillSetChangedEventArgs : GameEventArgs
{

    public bool IsLosingSkill
    {
        get;
        set;
    }

    public List<ISkill> Skills
    {
        get;
        set;
    } = [];
}

public class PlayerIsAboutToUseOrPlayCardEventArgs : GameEventArgs
{
    public PlayerIsAboutToUseOrPlayCardEventArgs()
    {
    }

    public ICardUsageVerifier Verifier
    {
        get;
        set;
    }
}
