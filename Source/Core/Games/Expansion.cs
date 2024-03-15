using Sanguosha.Core.Cards;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Skills;
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

    public static Card CreateHeroCard<Skill1>(string name, bool isMale, Allegiance a, int health) where Skill1 : ISkill, new()
    {
        return new Card(SuitType.None, -1, new HeroCardHandler(new Hero(name, isMale, a, health, new Skill1())));
    }

    public static Card CreateHeroCard<Skill1, Skill2>(string name, bool isMale, Allegiance a, int health) where Skill1 : ISkill, new() where Skill2 : ISkill, new()
    {
        return new Card(SuitType.None, -1, new HeroCardHandler(new Hero(name, isMale, a, health, new Skill1(), new Skill2())));
    }
}
