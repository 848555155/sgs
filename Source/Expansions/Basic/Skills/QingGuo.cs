﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 倾国-你可以将一张黑色手牌当【闪】使用或打出。
/// </summary>
public class QingGuo : OneToOneCardTransformSkill
{
    public QingGuo()
    {
        HandCardOnly = true;
    }

    public override bool VerifyInput(Card card, object arg)
    {
        return card.SuitColor == SuitColorType.Black;
    }

    public override CardHandler PossibleResult => new Shan();
}
