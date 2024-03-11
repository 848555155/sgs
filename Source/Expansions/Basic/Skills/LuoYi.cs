using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 裸衣-摸牌阶段摸牌时，你可以少摸一张牌，则你使用【杀】或【决斗】（你为伤害来源时）造成的伤害+1，直到回合结束。
/// </summary>
public class LuoYi : TriggerSkill
{
    public static PlayerAttribute Naked = PlayerAttribute.Register("Naked", true);

    public LuoYi()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { p[Player.DealAdjustment]--; p[Naked] = 1; },
            TriggerCondition.OwnerIsSource
        );
        var trigger2 = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return p[Naked] == 1 && a.ReadonlyCard != null && (a.ReadonlyCard.Type is Sha || a.ReadonlyCard.Type is JueDou); },
            (p, e, a) => { (a as DamageEventArgs).Magnitude++; },
            TriggerCondition.OwnerIsSource
        )
        { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
        Triggers.Add(GameEvent.DamageElementConfirmed, trigger2);
        IsAutoInvoked = false;
    }

}