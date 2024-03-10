﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Games;

public class RoleCardHandler : CardHandler
{
    public override object Clone()
    {
        return new RoleCardHandler(this.Role);
    }

    public override CardCategory Category
    {
        get { return CardCategory.Unknown; }
    }

    protected override void Process(Players.Player source, Players.Player dest, ICard card, ReadOnlyCard cardr, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public override UI.VerifierResult Verify(Players.Player source, ICard card, List<Players.Player> targets, bool isLooseVerify)
    {
        throw new NotImplementedException();
    }

    public Role Role { get; set; }

    public RoleCardHandler(Role r)
    {
        Role = r;
    }

    public override string Name
    {
        get
        {
            return Role.ToString();
        }
    }
}
public class UnknownRoleCardHandler : RoleCardHandler
{
    public UnknownRoleCardHandler() : base(Role.Unknown)
    {
    }

    public override string Name
    {
        get
        {
            return _cardTypeString;
        }
    }

    private static readonly string _cardTypeString = "UnknownRole";
}
