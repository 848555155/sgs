using Sanguosha.Core.Heroes;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills;

public abstract class PassiveSkill : ISkill
{
    private Players.Player owner;
    public ISkill LinkedSkill { get; protected set; }


    /// <summary>
    /// Owner of the skill.
    /// </summary>
    /// <remarks>If you override this, you should install triggers when owner changes.</remarks>
    public virtual Players.Player Owner
    {
        get { return owner; }
        set
        {
            if (owner == value) return;
            if (owner != null)
            {
                UninstallTriggers(owner);
            }
            owner = value;
            if (owner != null)
            {
                InstallTriggers(owner);
            }
            if (LinkedSkill != null)
            {
                LinkedSkill.HeroTag = HeroTag;
                LinkedSkill.Owner = value;
            }
        }
    }

    protected abstract void InstallTriggers(Players.Player owner);

    protected abstract void UninstallTriggers(Players.Player owner);

    public bool IsRulerOnly { get; protected set; }
    public bool IsSingleUse { get; protected set; }
    public bool IsAwakening { get; protected set; }
    public bool IsEnforced { get; protected set; }

    public Hero HeroTag { get; set; }

    private bool? isAutoInvoked = false;
    public bool? IsAutoInvoked
    {
        get
        {
            if (IsEnforced) return null;
            else return isAutoInvoked;
        }
        set
        {
            if (isAutoInvoked == value) return;
            isAutoInvoked = value;
        }
    }

    public object Clone()
    {
        var skill = Activator.CreateInstance(GetType()) as PassiveSkill;
        skill.Owner = Owner;
        skill.IsAutoInvoked = IsAutoInvoked;
        return skill;
    }
    
    public UiHelper Helper { get; private set; } = new();
}
