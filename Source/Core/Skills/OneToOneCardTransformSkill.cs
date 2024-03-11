using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills;

public abstract class OneToOneCardTransformSkill : CardTransformSkill
{
    public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
    {
        card = null;
        if (cards == null || cards.Count < 1)
        {
            return VerifierResult.Partial;
        }
        if (cards.Count > 1)
        {
            return VerifierResult.Fail;
        }
        if (cards[0].Place.DeckType != DeckType.None && cards[0].Owner != Owner && !(Helper.OtherDecksUsed.Count != 0 && Helper.OtherDecksUsed.Contains(cards[0].Place.DeckType)))
        {
            return VerifierResult.Fail;
        }
        if (HandCardOnly)
        {
            if (cards[0].Place.DeckType != DeckType.Hand)
            {
                return VerifierResult.Fail;
            }
        }
        if (VerifyInput(cards[0], arg))
        {
            card = new CompositeCard
            {
                Subcards = new List<Card>(cards),
                Type = PossibleResult
            };
            return VerifierResult.Success;
        }
        return VerifierResult.Fail;
    }

    public abstract bool VerifyInput(Card card, object arg);

    public bool HandCardOnly
    {
        get;
        protected set;
    }

    /// <summary>
    /// 卡牌转换技能可以转换成的卡牌类型。
    /// </summary>
    public abstract CardHandler PossibleResult { get; }

    public override List<CardHandler> PossibleResults => [PossibleResult];
}
