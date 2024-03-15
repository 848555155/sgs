using Sanguosha.Core.Games;
using Sanguosha.Core.Network;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;
using Sanguosha.Lobby.Core;
using System.Diagnostics;
using System.Net;

namespace Sanguosha.Lobby.Server;

public class GameService
{
    public delegate Task GameEndCallback(string roomId);
    public static void StartGameService(IPAddress IP, GameSettings setting, string roomId, GameEndCallback callback, out int portNumber)
    {
        int totalNumberOfPlayers = setting.TotalPlayers;
        int timeOutSeconds = setting.TimeOutSeconds;
#if DEBUG
        Trace.Listeners.Clear();

        var twtl = new TextWriterTraceListener(Path.Combine(Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.FriendlyName + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".txt"))
        {
            Name = "TextLogger",
            TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
        };

        var ctl = new ConsoleTraceListener(false)
        {
            TraceOutputOptions = TraceOptions.DateTime
        };

        Trace.Listeners.Add(twtl);
        Trace.Listeners.Add(ctl);
        Trace.AutoFlush = true;
        Trace.WriteLine("Log starting");
        Trace.Listeners.Add(new ConsoleTraceListener());
#endif
        Game game = setting.GameType == GameType.Pk1V1 ? new Pk1v1Game() : new RoleGame();
        game.Settings = setting;
        Sanguosha.Core.Network.Server server;
        server = new Sanguosha.Core.Network.Server(game, totalNumberOfPlayers, IP);
        portNumber = server.IpPort;
        for (int i = 0; i < totalNumberOfPlayers; i++)
        {
            var player = new Player
            {
                Id = i
            };
            game.Players.Add(player);
            IPlayerProxy proxy = new ServerNetworkProxy(server, i)
            {
                TimeOutSeconds = timeOutSeconds,
                HostPlayer = player
            };
            game.UiProxies.Add(player, proxy);
        }
        var pxy = new GlobalServerProxy(game, game.UiProxies)
        {
            TimeOutSeconds = timeOutSeconds
        };
        game.GlobalProxy = pxy;
        game.NotificationProxy = new DummyNotificationProxy();

        game.GameServer = server;
        Task.Factory.StartNew(async () =>
        {
            Thread.CurrentThread.IsBackground = true;
#if !DEBUG
            try
            {
#endif
                game.Run();
#if !DEBUG
            }
            catch (Exception)
            {
            }
#endif
            try
            {
                await callback(roomId);
            }
            catch (Exception)
            {
            }
        });
    }

}
