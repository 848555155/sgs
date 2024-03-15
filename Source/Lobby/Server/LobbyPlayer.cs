using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

public class LobbyPlayer(Account account, string connectedId)
{
    public string ConnectedId { get; set; } = connectedId;
    public Account Account { get; set; } = account;
    public ServerRoom? CurrentRoom { get; set; }
    public ServerRoom? CurrentSpectatingRoom { get; set; }
    public DateTime LastAction { get; set; }

}
