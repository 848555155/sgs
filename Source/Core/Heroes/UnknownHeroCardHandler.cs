namespace Sanguosha.Core.Heroes;

public class UnknownHeroCardHandler : HeroCardHandler
{
    public UnknownHeroCardHandler() : base(null)
    {
    }
    public override string Name => _cardTypeString;

    private static readonly string _cardTypeString = "UnknownHero";
}
