﻿#define NETWORKING
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Sanguosha.UI.Controls;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.UI;
using System.Threading;
using Sanguosha.Core.Network;
using System.Diagnostics;
using System.IO;

namespace Sanguosha.UI.Main
{
    /// <summary>
    /// Interaction logic for MainGame.xaml
    /// </summary>
    public partial class MainGame : Page
    {
        public MainGame()
        {
            this.InitializeComponent();            
            // Insert code required on object creation below this point.
        }

        private Client _networkClient;

        public Client NetworkClient
        {
            get { return _networkClient; }
            set { _networkClient = value; }
        }

        int MainSeat = 0;
        const int numberOfHeros = 3;
        private void InitGame()
        {
            TextWriterTraceListener twtl = new TextWriterTraceListener(System.IO.Path.Combine(Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.FriendlyName + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".txt"));
            twtl.Name = "TextLogger";
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;

            Trace.WriteLine("Log starting");
            _game = new RoleGame(1);
#if NETWORKING
            
            MainSeat = (int)NetworkClient.Receive();
            NetworkClient.SelfId = MainSeat;
#endif
            foreach (var g in GameEngine.Expansions.Values)
            {
                _game.LoadExpansion(g);
            }
#if NETWORKING
            ClientNetworkUiProxy activeClientProxy = null;
            for (int i = 0; i < numberOfHeros; i++)
#else
            for (int i = 0; i < 8; i++)
#endif
            {
                Player player = new Player();
                player.Id = i;
                _game.Players.Add(player);
                if (i == 1)
                {
                    player.IsFemale = true;
                }
                else
                {
                    player.IsMale = true;
                }
            }
#if NETWORKING
            _game.GameClient = NetworkClient;
            _game.GameServer = null;
            _game.IsClient = true;
#else
            _game.GlobalProxy = new GlobalDummyProxy();
#endif
            GameViewModel gameModel = new GameViewModel();
            gameModel.Game = _game;
            gameModel.MainPlayerSeatNumber = MainSeat;
            gameView.DataContext = gameModel;
            _game.NotificationProxy = gameView;
            List<ClientNetworkUiProxy> inactive = new List<ClientNetworkUiProxy>();
            for (int i = 0; i < _game.Players.Count; i++)
            {
                var player = gameModel.PlayerModels[i].Player;                
#if NETWORKING
                var proxy = new ClientNetworkUiProxy(
                            new AsyncUiAdapter(gameModel.PlayerModels[i]), NetworkClient, i == 0);
                proxy.HostPlayer = player;
                proxy.TimeOutSeconds = 15;
                if (i == 0)
                {
                    activeClientProxy = proxy;
                }
                else
                {
                    inactive.Add(proxy);
                }
#else
                var proxy = new AsyncUiAdapter(gameModel.PlayerModels[i]);
#endif
                _game.UiProxies.Add(player, proxy);
            }
#if NETWORKING
            _game.GlobalProxy = new GlobalClientUiProxy(_game, activeClientProxy, inactive);
#endif
        }

        private Game _game;
        Thread gameThread;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitGame();
            gameThread = new Thread(_game.Run) { IsBackground = true };
            gameThread.Start();
        }

    }
}
