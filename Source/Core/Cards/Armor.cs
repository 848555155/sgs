using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Cards;


public abstract class Armor : Equipment
{
    /// <summary>
    /// 玩家无视防具
    /// </summary>
    /// <remarks>高顺</remarks>
    public static readonly PlayerAttribute PlayerIgnoreArmor = PlayerAttribute.Register(nameof(PlayerIgnoreArmor));

    /// <summary>
    /// 无条件防具无效
    /// </summary>
    /// <seealso cref="Sanguosha.Expansions.Woods.Skills.WuQian"/>
    public static readonly PlayerAttribute UnconditionalIgnoreArmor = PlayerAttribute.Register(nameof(UnconditionalIgnoreArmor), false);

    /// <summary>
    /// 卡牌无视所有防具
    /// </summary>
    /// <remarks></remarks>
    public static readonly CardAttribute IgnoreAllArmor = CardAttribute.Register(nameof(IgnoreAllArmor));

    /// <summary>
    /// 卡牌无视某个玩家防具
    /// </summary>
    /// <remarks>青钢剑</remarks>
    public static readonly CardAttribute IgnorePlayerArmor = CardAttribute.Register(nameof(IgnorePlayerArmor));
    public override CardCategory Category => CardCategory.Armor;

    public static bool ArmorIsValid(Player player, Player source, ReadOnlyCard card)
    {
        return (source is null || player[PlayerIgnoreArmor[source]] == 0) &&
               player[UnconditionalIgnoreArmor] == 0 &&
               (card is null || (card[IgnoreAllArmor] == 0 && card[IgnorePlayerArmor[player]] == 0));
    }

    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard cardr, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }
}
