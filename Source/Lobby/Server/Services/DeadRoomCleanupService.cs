using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

public class DeadRoomCleanupService(ILogger<DeadRoomCleanupService> logger, LobbyManager lobbyManager) : BackgroundService
{
    private readonly ILogger<DeadRoomCleanupService> logger = logger;
    private readonly LobbyManager lobbyManager = lobbyManager;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                DeadRoomCleanup();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("dead room cleanup service is canceled");
        }
    }

    private void DeadRoomCleanup()
    {
        foreach (var acc in lobbyManager.loggedInAccounts)
        {
            if (DateTime.Now.Subtract(acc.Value.LastAction).TotalSeconds >= 60 * 60)
            {
                acc.Value.CurrentRoom = null;
                foreach (var rm in lobbyManager.rooms)
                {
                    if (rm.Value.Room.Seats.Any(st => st.Account == acc.Value.Account)
                        || !rm.Value.Room.Seats.Any(st => st.State == SeatState.Host)
                        || rm.Value.Room.Seats.Any(st => !lobbyManager.loggedInAccounts.ContainsKey(st.Account.UserName))
                        )
                    {
                        lobbyManager.rooms.Remove(rm.Key, out var _);
                    }
                }
            }
        }
    }
}
