using Sanguosha.Core.Cards;

namespace Sanguosha.Core.Skills;

public interface IAdditionalTypedSkill : ISkill
{
    CardHandler AdditionalType { get; set; }
}
