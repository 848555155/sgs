using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills;

public interface ISkill : ICloneable
{
    Player Owner { get; set; }
    Hero HeroTag { get; set; }
    bool IsRulerOnly { get; }
    bool IsSingleUse { get; }
    bool IsAwakening { get; }
    bool IsEnforced { get; }
    UiHelper Helper { get; }
}

