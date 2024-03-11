using Sanguosha.Core.Heroes;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills;

public enum CheatType
{
    Card,
    Skill,
}

public class CheatSkill : ISkill
{
    public CheatType CheatType { get; set; }
    public int CardId { get; set; }

    /// <summary>
    /// Sets/gets name of the skill to be acquired by the CheatSkill.
    /// </summary>
    public string SkillName { get; set; }

    public Players.Player Owner { get; set; }

    public bool IsRulerOnly => false;

    public bool IsSingleUse => false;

    public bool IsAwakening => false;

    public bool IsEnforced => false;

    public object Clone()
    {
        var skill = Activator.CreateInstance(GetType()) as CheatSkill;
        skill.Owner = Owner;
        skill.CheatType = CheatType;
        skill.CardId = CardId;
        skill.SkillName = SkillName;
        return skill;
    }

    public Hero HeroTag { get; set; } = null;

    public UiHelper Helper { get; private set; } = new();
}
