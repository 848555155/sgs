using Sanguosha.Core.Cards;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Games;

public class RoleCardHandler(Role r) : CardHandler
{
    public override object Clone()
    {
        return new RoleCardHandler(Role);
    }

    public override CardCategory Category => CardCategory.Unknown;

    protected override void Process(Players.Player source, Players.Player dest, ICard card, ReadOnlyCard cardr, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public override UI.VerifierResult Verify(Players.Player source, ICard card, List<Players.Player> targets, bool isLooseVerify)
    {
        throw new NotImplementedException();
    }

    public Role Role { get; set; } = r;

    public override string Name => Role.ToString();
}
public class UnknownRoleCardHandler() : RoleCardHandler(Role.Unknown)
{
    public override string Name => _cardTypeString;

    private static readonly string _cardTypeString = "UnknownRole";
}
