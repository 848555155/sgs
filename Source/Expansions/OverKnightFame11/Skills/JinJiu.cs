using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;

namespace Sanguosha.Expansions.OverKnightFame11.Skills;

/// <summary>
/// 禁酒-锁定技，你的【酒】均视为【杀】。
/// </summary>
public class JinJiu : EnforcedCardTransformSkill
{
    protected override bool CardVerifier(ICard card)
    {
        return card.Type is Jiu;
    }

    protected override void TransfromAction(Player Owner, ICard card)
    {
        card.Type = new RegularSha();
    }

    public JinJiu()
    {
        Decks.Add(DeckType.Hand);
    }
}
