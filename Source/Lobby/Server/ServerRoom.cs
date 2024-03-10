using Sanguosha.Lobby.Core;
using System.Collections.Generic;

namespace Sanguosha.Lobby.Server;

public class ServerRoom
{
    public Room Room { get; set; }
    public HashSet<string> Spectators { get; private set; } = [];
}
