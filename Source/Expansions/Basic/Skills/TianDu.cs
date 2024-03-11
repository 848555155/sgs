using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 天妒-在你的判定牌生效后，你可以获得此牌。
/// </summary>
public class TianDu : TriggerSkill
{
    private delegate bool _AskTianDuDelegate();

    private class AlwaysGetJudgeCardTrigger : GetJudgeCardTrigger
    {
        protected override bool IsCorrectJudgeAction(ISkill skill, ICard card)
        {
            if (askDel())
            {
                this.skill.NotifySkillUse();
                return true;
            }
            return false;
        }

        private readonly _AskTianDuDelegate askDel;
        private readonly TriggerSkill skill;
        public AlwaysGetJudgeCardTrigger(Player owner, _AskTianDuDelegate del, TriggerSkill skill) : base(owner, null, null, true)
        {
            askDel = del;
            this.skill = skill;
        }
    }

    public TianDu()
    {
        Triggers.Add(GameEvent.PlayerJudgeDone, new AlwaysGetJudgeCardTrigger(Owner, AskForSkillUse, this) { Priority = int.MinValue });
        IsAutoInvoked = true;
    }
}
