using Sanguosha.Core.Players;

namespace Sanguosha.Core.Skills;

public interface IRulerGivenSkill : ISkill
{
    Player Master { get; set; }
}
