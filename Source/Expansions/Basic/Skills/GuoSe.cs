using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 国色-出牌阶段，你可以将一张方块牌当【乐不思蜀】使用。
/// </summary>
public class GuoSe : OneToOneCardTransformSkill<LeBuSiShu>
{
    public override bool VerifyInput(Card card, object arg)
    {
        return card.Suit == SuitType.Diamond;
    }
}
