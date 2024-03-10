namespace Sanguosha.Lobby.Core;

public partial class Account
{
    public string Password { get; set; }

    public bool IsDead { get; set; }

    public LoginToken LoginToken { get; set; }
}
