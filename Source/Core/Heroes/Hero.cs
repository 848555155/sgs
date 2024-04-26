using CommunityToolkit.Mvvm.ComponentModel;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.Heroes;

public enum Allegiance
{
    Unknown,
    Shu,
    Wei,
    Wu,
    Qun,
    God
}

public partial class Hero : ObservableObject, ICloneable
{
    [ObservableProperty]
    private Allegiance allegiance;

    [ObservableProperty]
    private List<ISkill> _skills;

    public Player Owner { get; set; }

    public bool IsMale { get; set; }

    public int MaxHealth { get; set; }

    public string HeroConvertFrom { get; set; }

    public bool IsSpecialHero => HeroConvertFrom != string.Empty;

    public Hero(string name, bool isMale, Allegiance a, int health, List<ISkill> skills)
    {
        Allegiance = a;
        Skills = skills;
        Name = name;
        MaxHealth = health;
        IsMale = isMale;
        HeroConvertFrom = string.Empty;
    }
    public Hero(string name, bool isMale, Allegiance a, int health, params ISkill[] skills)
    {
        Allegiance = a;
        Skills = new List<ISkill>(skills);
        Name = name;
        MaxHealth = health;
        IsMale = isMale;
        HeroConvertFrom = string.Empty;
    }

    public string Name { get; set; }

    public object Clone()
    {
        var hero = new Hero(Name, IsMale, Allegiance, MaxHealth, new List<ISkill>())
        {
            HeroConvertFrom = this.HeroConvertFrom,
            Skills = []
        };
        foreach (var s in Skills)
        {
            hero.Skills.Add(s.Clone() as ISkill);
        }
        return hero;
    }

    public ISkill LoseSkill(string skillName)
    {
        foreach (var sk in Skills)
        {
            if (sk.GetType().Name == skillName)
            {
                return LoseSkill(sk);
            }
        }
        return null;
    }

    public ISkill LoseSkill(ISkill skill)
    {
        if (!Skills.Contains(skill)) return null;
        Skills.Remove(skill);
        OnPropertyChanged(nameof(Skills));
        skill.HeroTag = null;
        skill.Owner = null;
        return skill;
    }

    public void LoseAllSkills()
    {
        if (Skills.Count == 0) return;
        var backup = new List<ISkill>(Skills);
        Skills.Clear();
        OnPropertyChanged(nameof(Skills));
        foreach (var sk in backup)
        {
            sk.HeroTag = null;
            sk.Owner = null;
        }
    }
}
