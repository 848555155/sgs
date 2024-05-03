using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 武圣-你可以将一张红色牌当【杀】使用或打出。
/// </summary>
public class WuSheng : OneToOneCardTransformSkill<RegularSha>
{
    public override bool VerifyInput(Card card, object arg)
    {
        return card.SuitColor == SuitColorType.Red;
    }
}
