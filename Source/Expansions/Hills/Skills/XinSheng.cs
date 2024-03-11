using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Hills.Skills;

/// <summary>
/// 新生-每当你受到1点伤害后，可获得一张"化身牌"。
/// </summary>
public class XinSheng : TriggerSkill
{
    protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
    {
        if (Owner.IsFemale) return 1;
        return 0;
    }

    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        var args = eventArgs as DamageEventArgs;
        int damage = args.Magnitude;
        while (damage-- > 0)
        {
            if (AskForSkillUse())
            {
                HuaShen.AcquireHeroCard(Owner, HeroTag);
            }
            else
                break;
        }
    }

    public XinSheng()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            Run,
            TriggerCondition.OwnerIsTarget
        );
        Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
        IsAutoInvoked = true;
    }
}
