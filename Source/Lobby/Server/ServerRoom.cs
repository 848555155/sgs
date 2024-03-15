using Sanguosha.Lobby.Core;
using System.Collections.Immutable;

namespace Sanguosha.Lobby.Server;

/// <summary>
/// 服务器房间
/// </summary>
/// <param name="room"></param>
public class ServerRoom(Room room)
{
    /// <summary>
    /// 房间
    /// </summary>
    public Room Room { get; set; } = room;

    /// <summary>
    /// 旁观者
    /// </summary>
    public ImmutableHashSet<string> Spectators { get; } = [];
}
