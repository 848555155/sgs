using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 苦肉-出牌阶段，你可以失去1点体力，然后摸两张牌。
/// </summary>
public class KuRou : ActiveSkill
{
    public KuRou()
    {
        Helper.HasNoConfirmation = true;
    }

    public override VerifierResult Validate(GameEventArgs arg)
    {
        if (arg.Cards != null && arg.Cards.Count != 0)
        {
            return VerifierResult.Fail;
        }
        if (arg.Targets != null && arg.Targets.Count != 0)
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }

    public override bool Commit(GameEventArgs arg)
    {
        Game.CurrentGame.LoseHealth(Owner, 1);
        if (Owner == null || Owner.IsDead) return true;
        Game.CurrentGame.DrawCards(Owner, 2);
        return true;
    }
}
