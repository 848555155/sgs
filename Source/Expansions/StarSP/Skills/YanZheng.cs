﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.StarSP.Skills;

/// <summary>
/// 严整–若你的手牌数大于你的当前体力值，你可以将你装备区内的一张牌当【无懈可击】使用。
/// </summary>
public class YanZheng : OneToOneCardTransformSkill<WuXieKeJi>
{

    public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
    {
        card = null;
        if (Owner.HandCards().Count <= Owner.Health)
            return VerifierResult.Fail;
        return base.TryTransform(cards, arg, out card, isPlay);
    }

    public override bool VerifyInput(Card card, object arg)
    {
        return card.Place.DeckType == DeckType.Equipment;
    }
}
