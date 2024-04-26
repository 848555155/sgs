using System.Collections.Concurrent;

namespace Sanguosha.Lobby.Server;

/// <summary>
/// 大厅管理类
/// </summary>
public class LobbyManager(ILogger<LobbyManager> logger)
{
    public readonly ConcurrentDictionary<string, LobbyPlayer> loggedInAccounts = [];

    public readonly ConcurrentDictionary<string, ServerRoom> rooms = [];

    private readonly ILogger<LobbyManager> logger = logger;

}
