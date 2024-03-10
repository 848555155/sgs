﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Fire.Skills;

/// <summary>
/// 看破–你可以将你的任意一张黑色手牌当【无懈可击】使用。
/// </summary>
public class KanPo : OneToOneCardTransformSkill
{
    public KanPo()
    {
        HandCardOnly = true;
    }

    public override CardHandler PossibleResult
    {
        get { return new WuXieKeJi(); }
    }

    public override bool VerifyInput(Card card, object arg)
    {
        return card.SuitColor == SuitColorType.Black;
    }
}
