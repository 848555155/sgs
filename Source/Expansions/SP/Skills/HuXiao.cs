using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.SP.Skills;

/// <summary>    
/// ��Х-�����ڳ��ƽ׶�ʹ�õġ�ɱ�����������������򱾽׶�����Զ���ʹ��һ�š�ɱ����
/// </summary>
public class HuXiao : TriggerSkill
{
    public HuXiao()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return Game.CurrentGame.CurrentPhase == TurnPhase.Play && Game.CurrentGame.CurrentPlayer == p; },
            (p, e, a) => { p[Sha.AdditionalShaUsable]++; },
            TriggerCondition.OwnerIsSource
            )
        { AskForConfirmation = false };
        Triggers.Add(ShaCancelling.PlayerShaTargetDodged, trigger);
        IsAutoInvoked = null;
    }
}
