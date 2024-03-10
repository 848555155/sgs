using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Heroes;

public class HeroCardHandler(Hero h) : CardHandler, ICloneable
{
    public override object Clone()
    {
        Hero h = (Hero)Hero.Clone();
        var handler = new HeroCardHandler(h);
        return handler;
    }

    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        return targets == null || targets.Count == 0 ? VerifierResult.Success : VerifierResult.Fail;
    }

    public Hero Hero { get; set; } = h;

    public override CardCategory Category => CardCategory.Hero;

    public override string Name => Hero.Name;
}
