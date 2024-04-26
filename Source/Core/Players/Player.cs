using CommunityToolkit.Mvvm.ComponentModel;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Network;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Sanguosha.Core.Players;

public partial class Player : ObservableObject
{
    public Player()
    {
        isDead = false;
        Id = 0;
        IsMale = false;
        IsFemale = false;
        maxHealth = 0;
        health = 0;
        hero = hero2 = null;
        Attributes = [];
        equipmentSkills = [];
        additionalSkills = [];
        additionalUndeletableSkills = [];
        AssociatedPlayerAttributes = new Dictionary<PlayerAttribute, PlayerAttribute>();
        AssociatedCardAttributes = new Dictionary<CardAttribute, CardAttribute>();
    }

    public int Id { get; set; }

    /// <summary>
    /// 铁锁
    /// </summary>
    [ObservableProperty]
    private bool isIronShackled;

    /// <summary>
    /// 翻面
    /// </summary>
    [ObservableProperty]
    private bool isImprisoned;

    [ObservableProperty]
    private bool isDead;

    public bool IsMale { get; set; }

    public bool IsFemale { get; set; }

    [ObservableProperty]
    private int maxHealth;

    [ObservableProperty]
    private Allegiance allegiance;

    [ObservableProperty]
    private int health;

    public int LostHealth => MaxHealth - Math.Max(Health, 0);

    public Dictionary<PlayerAttribute, int> Attributes { get; }

    public int this[PlayerAttribute key]
    {
        get => !Attributes.TryGetValue(key, out int value) ? 0 : value;
        set
        {
            if (!Attributes.TryGetValue(key, out int v))
            {
                Attributes.Add(key, value);
            }
            else if (v == value)
            {
                return;
            }
            Attributes[key] = value;
            OnPropertyChanged(nameof(Attributes));
        }
    }

    private void SetHero(ref Hero hero, Hero value)
    {
        hero = value;
        if (hero != null)
        {
            foreach (var skill in hero.Skills)
            {
                skill.HeroTag = hero;
                skill.Owner = this;
            }
            Trace.Assert(hero.Owner == null);
            hero.Owner = this;
        }
        OnPropertyChanged(nameof(Skills));
    }

    [ObservableProperty]
    private Hero hero;

    [ObservableProperty]
    private Hero hero2;

    [ObservableProperty]
    private Role role;

    private readonly List<ISkill> additionalSkills;

    public IList<ISkill> AdditionalSkills => new ReadOnlyCollection<ISkill>(additionalSkills);

    /// <summary>
    /// These skills are NOT affected by ANY game event, such as 断肠
    /// </summary>
    private readonly List<ISkill> additionalUndeletableSkills;

    public IList<ISkill> AdditionalUndeletableSkills => new ReadOnlyCollection<ISkill>(additionalUndeletableSkills);


    public void AcquireAdditionalSkill(ISkill skill, Hero tag, bool undeletable = false)
    {
        skill.HeroTag = tag;
        skill.Owner = this;
        if (undeletable)
        {
            additionalUndeletableSkills.Add(skill);
        }
        else
        {
            additionalSkills.Add(skill);
        }
        OnPropertyChanged(nameof(Skills));
    }

    public void LoseAdditionalSkill(ISkill skill, bool undeletable = false)
    {
        skill.HeroTag = null;
        skill.Owner = null;
        if (undeletable)
        {
            Trace.Assert(additionalUndeletableSkills.Contains(skill));
            additionalUndeletableSkills.Remove(skill);
        }
        else
        {
            Trace.Assert(additionalSkills.Contains(skill));
            additionalSkills.Remove(skill);
        }
        OnPropertyChanged(nameof(Skills));
    }


    private readonly List<ISkill> equipmentSkills;

    public IList<ISkill> EquipmentSkills => new ReadOnlyCollection<ISkill>(equipmentSkills);

    public void AcquireEquipmentSkill(ISkill skill)
    {
        skill.Owner = this;
        equipmentSkills.Add(skill);
    }

    public void LoseEquipmentSkill(ISkill skill)
    {
        skill.Owner = null;
        Trace.Assert(equipmentSkills.Contains(skill));
        equipmentSkills.Remove(skill);
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks> UI use only!</remarks>
    public IList<ISkill> Skills
    {
        get
        {
            List<ISkill> s = [];
            if (Hero != null)
            {
                s.AddRange(Hero.Skills);
            }
            if (Hero2 != null)
            {
                s.AddRange(Hero2.Skills);
            }
            s.AddRange(additionalSkills);
            s.AddRange(additionalUndeletableSkills);
            return new ReadOnlyCollection<ISkill>(s);
        }
    }

    public IList<ISkill> ActionableSkills
    {
        get
        {
            List<ISkill> s = [];
            if (Hero != null)
            {
                s.AddRange(Hero.Skills);
            }
            if (Hero2 != null)
            {
                s.AddRange(Hero2.Skills);
            }
            s.AddRange(equipmentSkills);
            s.AddRange(additionalSkills);
            s.AddRange(additionalUndeletableSkills);
            return new ReadOnlyCollection<ISkill>(s);
        }
    }

    [ObservableProperty]
    private OnlineStatus _onlineStatus;

    [ObservableProperty]
    private bool isTargeted;

    public void LoseAllHeroSkills(Hero h)
    {
        Trace.Assert(h.Owner == this);
        var skills = new List<ISkill>(h.Skills);
        h.LoseAllSkills();
        if (skills.Count > 0)
        {
            SkillSetChangedEventArgs arg = new SkillSetChangedEventArgs();
            arg.Source = this;
            arg.IsLosingSkill = true;
            arg.Skills = skills;
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, arg);
        }
    }

    public void LoseAllHerosSkills()
    {
        Trace.Assert(Hero != null);
        var skills = new List<ISkill>(Hero.Skills);
        Hero.LoseAllSkills();
        if (Hero2 != null)
        {
            skills.AddRange(Hero2.Skills);
            Hero2.LoseAllSkills();
        }
        if (skills.Count > 0)
        {
            var arg = new SkillSetChangedEventArgs
            {
                Source = this,
                IsLosingSkill = true,
                Skills = skills
            };
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, arg);
        }
    }

    public ISkill LoseHeroSkill(ISkill skill, Hero heroTag)
    {
        Trace.Assert(heroTag != null && heroTag.Owner == this);
        ISkill sk = heroTag.LoseSkill(skill);
        if (sk != null)
        {
            var arg = new SkillSetChangedEventArgs
            {
                Source = this,
                IsLosingSkill = true
            };
            arg.Skills.Add(sk);
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, arg);
        }
        return sk;
    }

    public ISkill LoseHeroSkill(string skillName, Hero heroTag)
    {
        Trace.Assert(heroTag != null && heroTag.Owner == this);
        ISkill skill = heroTag.LoseSkill(skillName);
        if (skill != null)
        {
            var arg = new SkillSetChangedEventArgs
            {
                Source = this,
                IsLosingSkill = true
            };
            arg.Skills.Add(skill);
            Game.CurrentGame.Emit(GameEvent.PlayerSkillSetChanged, arg);
        }
        return skill;
    }

    internal IDictionary<PlayerAttribute, PlayerAttribute> AssociatedPlayerAttributes
    {
        get;
        private set;
    }

    internal IDictionary<CardAttribute, CardAttribute> AssociatedCardAttributes
    {
        get;
        private set;
    }

    public static readonly PlayerAttribute RangeMinus = PlayerAttribute.Register(nameof(RangeMinus), false);
    public static readonly PlayerAttribute RangePlus = PlayerAttribute.Register(nameof(RangePlus), false);
    public static readonly PlayerAttribute AttackRange = PlayerAttribute.Register(nameof(AttackRange), false);
    public static readonly PlayerAttribute DealAdjustment = PlayerAttribute.Register(nameof(DealAdjustment), true);
    public static readonly PlayerAttribute IsDying = PlayerAttribute.Register(nameof(IsDying));
    public static readonly PlayerAttribute SkipDeathComputation = PlayerAttribute.Register("SkipDyingComputation");
    public static readonly PlayerAttribute Awakened = PlayerAttribute.Register(nameof(Awakened), false, true);
    public static readonly PlayerAttribute DisconnectedStatus = PlayerAttribute.Register("Disconnected", false, false, true);
}
