using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

public class ServerRoom
{
    public Room Room { get; set; }
    public HashSet<string> Spectators { get; private set; } = [];
}
