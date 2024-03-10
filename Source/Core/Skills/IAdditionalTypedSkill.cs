namespace Sanguosha.Core.Skills;

public interface IAdditionalTypedSkill : ISkill
{
    Cards.CardHandler AdditionalType { get; set; }
}
