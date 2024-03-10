using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Skills;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Games;
using System.Collections.ObjectModel;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Network;

namespace Sanguosha.Core.Players;

public class Player : INotifyPropertyChanged
{
    public Player()
    {
        isDead = false;
        Id = 0;
        isMale = false;
        isFemale = false;
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

    private bool isIronShackled;

    /// <summary>
    /// 铁锁
    /// </summary>
    public bool IsIronShackled
    {
        get { return isIronShackled; }
        set
        {
            if (isIronShackled == value) return;
            isIronShackled = value;
            OnPropertyChanged("IsIronShackled");
        }
    }

    private bool isImprisoned;

    /// <summary>
    /// 翻面
    /// </summary>
    public bool IsImprisoned
    {
        get { return isImprisoned; }
        set
        {
            if (isImprisoned == value) return;
            isImprisoned = value;
            OnPropertyChanged("IsImprisoned");
        }
    }

    private bool isDead;

    public bool IsDead
    {
        get { return isDead; }
        set 
        {
            if (isDead == value) return;
            isDead = value;
            OnPropertyChanged("IsDead");
        }
    }

    private bool isMale;

    public bool IsMale
    {
        get { return isMale; }
        set { isMale = value; }
    }

    private bool isFemale;

    public bool IsFemale
    {
        get { return isFemale; }
        set { isFemale = value; }
    }

    private int maxHealth;

    public int MaxHealth
    {
        get { return maxHealth; }
        set 
        {
            if (maxHealth == value)
            {
                return;
            }
            maxHealth = value;
            OnPropertyChanged("MaxHealth");
        }
    }

    private Allegiance allegiance;

    public Allegiance Allegiance
    {
        get { return allegiance; }
        set
        {
            if (allegiance == value)
            {
                return;
            }
            allegiance = value;
            OnPropertyChanged("Allegiance");
        }
    }

    private int health;

    public int Health
    {
        get { return health; }
        set 
        {
            if (health == value)
            {
                return;
            }
            health = value;
            OnPropertyChanged("Health");
        }
    }

    public int LostHealth => maxHealth - Math.Max(health, 0);

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
            OnPropertyChanged("Attributes");
        }
    }

    private void SetHero(ref Hero hero, Hero value)
    {
        if (hero != null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            hero.PropertyChanged -= handler;
        }
        hero = value;
        if (hero != null)
        {
            foreach (ISkill skill in hero.Skills)
            {
                skill.HeroTag = hero;
                skill.Owner = this;
            }
            Trace.Assert(hero.Owner == null);
            hero.Owner = this;          
            hero.PropertyChanged += PropertyChanged;
        }
        OnPropertyChanged("Skills");
    }

    private Hero hero;

    public Hero Hero
    {
        get { return hero; }
        set 
        {
            if (hero == value) return;
            SetHero(ref hero, value);
            OnPropertyChanged("Hero");
        }
    }

    private Hero hero2;

    public Hero Hero2
    {
        get { return hero2; }
        set
        {
            if (hero2 == value) return;
            SetHero(ref hero2, value);
            OnPropertyChanged("Hero2");
        }
    }

    private Role role;

    public Role Role
    {
        get { return role; }
        set
        {
            if (role == value) return;
            role = value;
            OnPropertyChanged("Role");
        }
    }

    private readonly List<ISkill> additionalSkills;

    public IList<ISkill> AdditionalSkills => new ReadOnlyCollection<ISkill>(additionalSkills);

    /// <summary>
    /// These skills are NOT affected by ANY game event, such as 断肠
    /// </summary>
    private readonly List<ISkill> additionalUndeletableSkills;

    public IList<ISkill> AdditionalUndeletableSkills
    {
        get { return new ReadOnlyCollection<ISkill>(additionalUndeletableSkills); }
    }


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
        OnPropertyChanged("Skills");
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
        OnPropertyChanged("Skills");
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

    private OnlineStatus _onlineStatus;

    public OnlineStatus OnlineStatus
    {
        get
        {
            return _onlineStatus;
        }
        set
        {
            if (_onlineStatus == value) return;
            _onlineStatus = value;
            OnPropertyChanged("OnlineStatus");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    // Create the OnPropertyChanged method to raise the event 
    protected void OnPropertyChanged(string name)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(name));
        }
    }

    private bool isTargeted;

    public bool IsTargeted
    {
        get { return isTargeted; }
        set { isTargeted = value; OnPropertyChanged("IsTargeted"); }
    }

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
