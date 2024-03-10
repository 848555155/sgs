using Sanguosha.Core.Cards;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Games;

public struct DelayedTriggerRegistration
{
    public GameEvent key;
    public Trigger trigger;
}

public abstract class Expansion
{
    /// <summary>
    /// Set of all available hero cards and hand cards in this expansion.
    /// </summary>
    public List<Card> CardSet { get; set; } = [];

    public List<DelayedTriggerRegistration> TriggerRegistration { get; set; }
}
