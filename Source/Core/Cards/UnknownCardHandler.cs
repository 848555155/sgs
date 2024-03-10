using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Cards;

public class UnknownCardHandler : CardHandler
{
    public override CardCategory Category => CardCategory.Unknown;

    protected override void Process(Players.Player source, Players.Player dest, ICard card, ReadOnlyCard cardr, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public override UI.VerifierResult Verify(Players.Player source, ICard card, List<Players.Player> targets, bool isLooseVerify)
    {
        throw new NotImplementedException();
    }

    public override string Name => _cardTypeString;

    private static readonly string _cardTypeString = "Unknown";
}
