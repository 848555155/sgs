﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Wind.Skills;

/// <summary>
/// 黄天―主公技，群雄角色可在他们各自的出牌阶段给你一张【闪】或【闪电】，每阶段限一次。 
/// </summary>
public class HuangTianGivenSkill : ActiveSkill, IRulerGivenSkill
{
    public override VerifierResult Validate(GameEventArgs arg)
    {
        if (Owner[HuangTianUsed[Master]] != 0)
        {
            return VerifierResult.Fail;
        }
        if (arg.Targets != null && arg.Targets.Count > 0)
        {
            return VerifierResult.Fail;
        }
        List<Card> cards = arg.Cards;
        if (cards != null && cards.Count > 1)
        {
            return VerifierResult.Fail;
        }
        if (cards != null && cards.Count > 0 && !(cards[0].Type is Shan || cards[0].Type is ShanDian))
        {
            return VerifierResult.Fail;
        }
        if (cards == null || cards.Count == 0)
        {
            return VerifierResult.Partial;
        }
        return VerifierResult.Success;
    }

    public static readonly PlayerAttribute HuangTianUsed = PlayerAttribute.Register("HuangTianUsed", true);

    public override bool Commit(GameEventArgs arg)
    {
        Owner[HuangTianUsed[Master]] = 1;
        Game.CurrentGame.HandleCardTransferToHand(Owner, Master, arg.Cards);
        return true;
    }

    public Player Master { get; set; }
}

public class HuangTian : RulerGivenSkillContainerSkill
{
    public HuangTian() : base(new HuangTianGivenSkill(), Allegiance.Qun)
    {
    }
}
