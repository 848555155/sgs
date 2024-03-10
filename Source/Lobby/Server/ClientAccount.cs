using Sanguosha.Lobby.Core;
using System.ServiceModel;

namespace Sanguosha.Lobby.Server;

public class ClientAccount
{
    public OperationContext OpContext { get; set; }
    public Account Account { get; set; }
    public ServerRoom CurrentRoom { get; set; }
    public ServerRoom CurrentSpectatingRoom { get; set; }
    public IGameClient CallbackChannel { get; set; }
    public LobbyServiceImpl LobbyService { get; set; }
    public DateTime LastAction { get; set; }
}
