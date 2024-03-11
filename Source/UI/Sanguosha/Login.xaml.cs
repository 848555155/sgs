﻿using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading;
using System.IO;
using Sanguosha.Core.Games;
using Sanguosha.Core.Network;
using Microsoft.Win32;
using System.ComponentModel;
using Sanguosha.UI.Controls;
using Sanguosha.Lobby.Core;
using Sanguosha.Lobby.Server;
using System.Net.NetworkInformation;
using System.Net;
using Sanguosha.Core.Utils;
using System.Diagnostics;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;

namespace Sanguosha.UI.Main;

/// <summary>
/// Interaction logic for Login.xaml
/// </summary>
public partial class Login : Page, IDisposable
{
    public static int DefaultLobbyPort = 6080;

    private static readonly string[] _dictionaryNames = new string[] { "Cards.xaml", "Skills.xaml", "Game.xaml" };

    private void _LoadResources(string folderPath)
    {
        try
        {
            var files = Directory.GetFiles(string.Format("{0}/Texts", folderPath));
            foreach (var filePath in files)
            {
                if (!_dictionaryNames.Any(fileName => filePath.Contains(fileName))) continue;
                try
                {
                    Uri uri = new Uri(string.Format("pack://siteoforigin:,,,/{0}", filePath));
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources;component/Lobby.xaml") });
            GameSoundLocator.Initialize();
        }
        catch (DirectoryNotFoundException)
        {
        }
        PreloadCompleted = true;
        _UpdateStartButton();
    }

    public static string ExpansionFolder = "./";
    public static string ResourcesFolder = "Resources";

    internal static bool PreloadCompleted { get; set; } = false;

    private bool _startButtonEnabled;

    internal bool StartButtonEnabled
    {
        get { return _startButtonEnabled; }
        set
        {
            _startButtonEnabled = value;
            _UpdateStartButton();
        }
    }

    private void _UpdateStartButton()
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            startButton.IsEnabled = _startButtonEnabled && PreloadCompleted;
        }
        else
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                startButton.IsEnabled = _startButtonEnabled && PreloadCompleted;
            });
        }
    }

    private readonly Thread loadingThread;

    private void _Load()
    {
        _LoadResources(ResourcesFolder);

        GameEngine.LoadExpansions(ExpansionFolder);

    }

    private static Login _instance;

    public static Login Instance
    {
        get
        {
            _instance ??= new Login();
            return _instance;
        }
    }

    public Login()
    {
        _startButtonEnabled = true; // @todo: change this.
        if (!PreloadCompleted)
        {
            loadingThread = new Thread(_Load) { IsBackground = true };
            loadingThread.Start();
            InitializeComponent();
        }
        else
        {
            InitializeComponent();
            _UpdateStartButton();
        }
        tab0UserName.Text = Properties.Settings.Default.LastUserName;
        tab0HostName.Text = Properties.Settings.Default.LastHostName;
        tab1Port.Text = DefaultLobbyPort.ToString();

        Application.Current.SessionEnding += (s, e) => { _LogOut(); };
        Application.Current.Exit += (s, e) => { _LogOut(); };
    }

    private System.ServiceModel.ChannelFactory _channelFactory;
    //DuplexChannelFactory<ILobbyService> _channelFactory;

    private void _LogOut()
    {
        if (LobbyViewModel.Instance.Connection != null)
        {
            try
            {
                LobbyViewModel.Instance.Connection.Logout(new Empty());
                LobbyViewModel.Instance.Connection = null;
            }
            catch (Exception)
            {
            }
        }

        if (_channelFactory != null)
        {
            try
            {
                _channelFactory.Close();
            }
            catch (Exception)
            {
                _channelFactory.Abort();
            }
            _channelFactory = null;
        }

    }

    private void startButton_Click(object sender, RoutedEventArgs e)
    {
        startButton.IsEnabled = false;
        if (loginTab.SelectedIndex == 0)
        {
            _startClient();
        }
        else if (loginTab.SelectedIndex == 1)
        {
            _startServer();
        }
        else if (loginTab.SelectedIndex == 2)
        {
            _startSinglePlayer();
        }
    }

    public static Mutex appMutex = null;

    private void _startClient()
    {
        _userName = tab0UserName.Text;
        _passWd = tab0Password.Password;
        Properties.Settings.Default.LastHostName = tab0HostName.Text;
        Properties.Settings.Default.LastUserName = tab0UserName.Text;
        Properties.Settings.Default.Save();
#if !DEBUG
        bool createdNew;
        if (appMutex == null)
        {
            appMutex = new System.Threading.Mutex(true, "Sanguosha", out createdNew);
            ///if creation of mutex is successful
            if (!createdNew && tab0HostName.Text != "127.0.0.1")
            {
                appMutex = null;
                _Warn("You already have another Sanguosha running!");
                return;
            }
        }
#endif

        if (string.IsNullOrEmpty(_userName))
        {
            _Warn("Please provide a username");
            return;
        }

        busyIndicator.BusyContent = Resources["Busy.ConnectServer"];
        busyIndicator.IsBusy = true;
        Lobby.Core.Lobby.LobbyClient server = null;
        LoginToken token = new LoginToken();
        string reconnect = null;
        _hostName = tab0HostName.Text;
        if (!_hostName.Contains(":"))
        {
            _hostName = _hostName + ":" + DefaultLobbyPort;
        }

        BackgroundWorker worker = new BackgroundWorker();

        worker.DoWork += (o, ea) =>
        {
            try
            {
                ea.Result = LoginStatus.UnknownFailure;

                _LogOut();
                var lobbyModel = LobbyViewModel.Instance;
                var channel = GrpcChannel.ForAddress(string.Format("https://localhost:50456"));
                server = new Lobby.Core.Lobby.LobbyClient(channel);
                // todo change to GRPC
                //var binding = new NetTcpBinding();
                //binding.Security.Mode = SecurityMode.None;
                //var endpoint = new EndpointAddress(string.Format("net.tcp://{0}/GameService", _hostName));
                //_channelFactory = new DuplexChannelFactory<ILobbyService>(typeof(LobbyViewModel), binding, endpoint);
                //server = _channelFactory.CreateChannel();

                //_channelFactory.Faulted += channelFactory_Faulted;
                Account ret;
                var stat = server.Login(new()
                {
                    Version = Misc.ProtocolVersion,
                    Username = _userName,
                    Hash = _passWd
                });
                ret = stat.RetAccount;
                reconnect = stat.ReconnectionString;
                token = new() { TokenString = Guid.Parse(stat.TokenString) };
                if (stat.Status == LoginStatus.Success)
                {
                    LobbyViewModel.Instance.CurrentAccount = ret;

                    if (reconnect != null)
                    {
                        Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
                        {
                            MainGame.BackwardNavigationService = this.NavigationService;
                            busyIndicator.BusyContent = Resources["Busy.Reconnecting"];
                        });
                    }
                }
                ea.Result = stat;
            }
            catch (Exception e)
            {
                string s = e.StackTrace;
            }
        };

        worker.RunWorkerCompleted += (o, ea) =>
        {
            bool success = false;
            if ((LoginStatus)ea.Result == LoginStatus.Success)
            {
                LobbyView lobby = LobbyView.Instance;
                LobbyView.Instance.OnNavigateBack += lobby_OnNavigateBack;
                var lobbyModel = LobbyViewModel.Instance;
                lobbyModel.Connection = server;
                lobbyModel.LoginToken = token;

                if (reconnect == null)
                {
                    this.NavigationService.Navigate(lobby);
                    busyIndicator.IsBusy = false;
                }
                else
                {
                    lobbyModel.NotifyGameStart(reconnect, token);
                    busyIndicator.IsBusy = true;
                }

                success = true;
            }
            if (!success)
            {
                if ((LoginStatus)ea.Result == LoginStatus.InvalidUsernameAndPassword)
                {
                    MessageBox.Show("Invalid Username and Password");
                    busyIndicator.IsBusy = false;
                }
                else if ((LoginStatus)ea.Result == LoginStatus.OutdatedVersion)
                {
                    // MessageBox.Show("Outdated version. Please update");
                    busyIndicator.BusyContent = Resources["Busy.Updating"];
                }
                else
                {
                    MessageBox.Show("Cannot connect to server.");
                    busyIndicator.IsBusy = false;
                }
            }
            startButton.IsEnabled = true;
        };

        worker.RunWorkerAsync();
    }

    private string _hostName;
    private string _userName;
    private string _passWd;

    private void channelFactory_Faulted(object sender, EventArgs e)
    {
        // todo change to GRPC
        //var binding = new NetTcpBinding();
        //binding.Security.Mode = SecurityMode.None;
        //var endpoint = new EndpointAddress(string.Format("net.tcp://{0}/GameService", _hostName));
        //var channelFactory = new DuplexChannelFactory<ILobbyService>(typeof(LobbyViewModel), binding, endpoint);
        //LobbyViewModel.Instance.Connection = channelFactory.CreateChannel();

        var channel = GrpcChannel.ForAddress(string.Format("https://localhost:50456"));
        LobbyViewModel.Instance.Connection = new Lobby.Core.Lobby.LobbyClient(channel);

        Account ret;
        string reconnect;
        LoginToken token;
        var stat = LobbyViewModel.Instance.Connection.Login(new()
        {
            Version = Misc.ProtocolVersion,
            Username = _userName,
            Hash = _passWd
        });
        ret = stat.RetAccount;
        reconnect = stat.ReconnectionString;
        token = new() { TokenString = Guid.Parse(stat.TokenString) };
        if (stat.Status == LoginStatus.Success)
        {
            LobbyViewModel.Instance.CurrentAccount = ret;
        }
    }

    private void _createAccount()
    {
        string userName = tab0UserName.Text;
        string passwd = tab0Password.Password;
        Properties.Settings.Default.LastHostName = tab0HostName.Text;
        Properties.Settings.Default.LastUserName = tab0UserName.Text;
        Properties.Settings.Default.Save();
#if !DEBUG
        if (string.IsNullOrEmpty(userName))
        {
            _Warn("Please provide a username");
            return;
        }
#endif
        busyIndicator.BusyContent = Resources["Busy.ConnectServer"];
        busyIndicator.IsBusy = true;
        string hostName = tab0HostName.Text;
        if (!hostName.Contains(':'))
        {
            hostName = hostName + ":" + DefaultLobbyPort;
        }

        BackgroundWorker worker = new BackgroundWorker();

        worker.DoWork += (o, ea) =>
        {
            try
            {
                ea.Result = LoginStatus.UnknownFailure;
                // todo change to GRPC
                //var binding = new NetTcpBinding();
                //binding.Security.Mode = SecurityMode.None;
                //var endpoint = new EndpointAddress(string.Format("net.tcp://{0}/GameService", hostName));
                //var channelFactory = new DuplexChannelFactory<ILobbyService>(typeof(LobbyViewModel), binding, endpoint);
                //server = channelFactory.CreateChannel();
                var channel = GrpcChannel.ForAddress(string.Format("https://localhost:50456"));
                var server = new Lobby.Core.Lobby.LobbyClient(channel);
                var stat = server.CreateAccount(new()
                {
                    UserName = userName, 
                    P = passwd
                });
                ea.Result = stat.LoginStatus;
            }
            catch (Exception e)
            {
                string s = e.StackTrace;
            }
        };

        worker.RunWorkerCompleted += (o, ea) =>
        {
            busyIndicator.IsBusy = false;
            switch ((LoginStatus)ea.Result)
            {
                case LoginStatus.Success:
                    MessageBox.Show("Account created successfully");
                    break;
                case LoginStatus.OutdatedVersion:
                    MessageBox.Show("Outdated version. Please update.");
                    break;
                case LoginStatus.InvalidUsernameAndPassword:
                    MessageBox.Show("Invalid Username and Password.");
                    break;
                default:
                    MessageBox.Show("Failed to launch client.");
                    break;
            }
        };

        worker.RunWorkerAsync();
    }

    private void _Warn(string message)
    {
        MessageBox.Show(message, "Error");
    }

    private void _startServer()
    {
        LobbyService gameService = null;
        //CoreWCF.ServiceHostBase host = null;
        IPAddress serverIp = tab1IpAddresses.SelectedItem as IPAddress;
        if (serverIp == null)
        {
            _Warn("Please select an IP address");
            return;
        }
        int portNumber;
        if (!int.TryParse(tab1Port.Text, out portNumber))
        {
            _Warn("Please enter a legal port number");
            return;
        }
        IPAddress publicIP;
        if (!IPAddress.TryParse(tab1PublicIP.Text, out publicIP))
        {
            publicIP = serverIp;
        }
        busyIndicator.BusyContent = Resources["Busy.LaunchServer"];
        busyIndicator.IsBusy = true;

        //client.Start(isReplay, FileStream = file.open(...))
        BackgroundWorker worker = new BackgroundWorker();
        bool hasDatabase = (tab1EnableDb.IsChecked == true);

        worker.DoWork += (o, ea) =>
        {
            try
            {
                ea.Result = false;
                if (hasDatabase) LobbyService.EnableDatabase();
                LobbyService.HostingIp = serverIp;
                LobbyService.PublicIp = publicIP;
                // todo move to new project
                //host = new CoreWCF.ServiceHostBase(typeof(LobbyServiceImpl));
                //, new Uri[] { new Uri(string.Format("net.tcp://{0}:{1}/GameService", serverIp, portNumber)) });

                // todo change to GRPC
                //var binding = new NetTcpBinding();
                //binding.Security.Mode = SecurityMode.None;
                //binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
                //binding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
                //binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
                //binding.MaxBufferPoolSize = Misc.MaxBugReportSize;
                //binding.MaxBufferSize = Misc.MaxBugReportSize;
                //binding.MaxReceivedMessageSize = Misc.MaxBugReportSize;
                // todo move to new project
                //host.AddServiceEndpoint(typeof(ILobbyService), binding, string.Format("net.tcp://{0}:{1}/GameService", serverIp, portNumber));
                //host.Open();
                
                ea.Result = true;
            }
            catch (Exception)
            {
            }
        };

        worker.RunWorkerCompleted += (o, ea) =>
        {
            busyIndicator.IsBusy = false;
            if ((bool)ea.Result)
            {
                var serverPage = new ServerPage
                {
                    // todo
                    //serverPage.Host = host;
                    GameService = gameService
                };
                this.NavigationService.Navigate(serverPage);
            }
            else
            {
                MessageBox.Show("Failed to launch server");
            }
            startButton.IsEnabled = true;
        };

        worker.RunWorkerAsync();
    }

    private void _startSinglePlayer()
    {
        MainGame game = null;
        game = new MainGame();
        game.OnNavigateBack += game_OnNavigateBack;
        game.NetworkClient = null;
        MainGame.BackwardNavigationService = this.NavigationService;
        game.Start();
        startButton.IsEnabled = true;
    }

    private void btnReplay_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists("./Replays"))
        {
            Directory.CreateDirectory("./Replays");
        }
        OpenFileDialog dlg = new OpenFileDialog();
        dlg.InitialDirectory = Directory.GetCurrentDirectory() + "\\Replays";
        dlg.DefaultExt = ".sgs"; // Default file extension
        dlg.Filter = "Replay File (.sgs)|*.sgs|Crash Report File (.rpt)|*.rpt|All Files (*.*)|*.*"; // Filter files by extension
        bool? result = dlg.ShowDialog();
        if (result != true) return;

        string fileName = dlg.FileName;

        Client client;
        MainGame game = null;
        try
        {
            client = new Client();
            game = new MainGame();
            game.OnNavigateBack += game_OnNavigateBack;
            Stream stream = File.Open(fileName, FileMode.Open);
            byte[] seed = new byte[8];
            stream.Seek(-16, SeekOrigin.End);
            stream.Read(seed, 0, 8);
            if (Encoding.Default.GetString(seed).Equals(Misc.MagicAnimal.ToString("X8")))
            {
                stream.Read(seed, 0, 8);
                game.HasSeed = Convert.ToInt32(Encoding.Default.GetString(seed), 16);
            }
            stream.Seek(0, SeekOrigin.Begin);

            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            int length = BitConverter.ToInt32(bytes, 0);
            if (length != 0)
            {
                byte[] msg = new byte[length];
                stream.Read(msg, 0, length);
                UnicodeEncoding uniEncoding = new UnicodeEncoding();
                MessageBox.Show(new String(uniEncoding.GetChars(msg)));
            }
            client.StartReplay(stream);
            game.NetworkClient = client;
        }
        catch (Exception)
        {
            MessageBox.Show("Failed to open replay file.");
            return;
        }
        if (game != null)
        {
            MainGame.BackwardNavigationService = this.NavigationService;
            game.Start();
            // this.NavigationService.Navigate(game);
        }
    }

    private static void game_OnNavigateBack(object sender, NavigationService service)
    {
        MainGame game = sender as MainGame;
        if (game != null) game.OnNavigateBack -= game_OnNavigateBack;
        Instance._LogOut();
        Trace.Assert(service != null);
        service.Navigate(Instance);
    }

    private static void lobby_OnNavigateBack(object sender, NavigationService service)
    {
        LobbyView lobby = sender as LobbyView;
        if (lobby != null) lobby.OnNavigateBack -= lobby_OnNavigateBack;
        Instance._LogOut();
        Trace.Assert(service != null);
        service.Navigate(Instance);
    }

    #region Network Related

    private void _ListAdaptors()
    {
        tab1Adaptors.Items.Clear();
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (!tab1ShowAllAdaptor.IsChecked == true)
            {
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                {
                    continue;
                }
            }
            tab1Adaptors.Items.Add(nic);
        }
    }

    private void _ListIpAddresses()
    {
        tab1IpAddresses.Items.Clear();
        NetworkInterface ni = tab1Adaptors.SelectedItem as NetworkInterface;
        if (ni == null) return;
        foreach (var ip in ni.GetIPProperties().UnicastAddresses)
        {
            tab1IpAddresses.Items.Add(ip.Address);
        }
    }

    private void tab1ShowAllAdaptor_Checked(object sender, RoutedEventArgs e)
    {
        _ListAdaptors();
    }

    private void tab1ShowAllAdaptor_Unchecked(object sender, RoutedEventArgs e)
    {
        _ListAdaptors();

    }

    private bool _adaptorSearched;
    private void loginTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_adaptorSearched && loginTab.SelectedIndex == 1)
        {
            _ListAdaptors();
            _adaptorSearched = true;
        }
    }

    private void tab1Adaptors_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ListIpAddresses();
    }

    private void tab1ClearDb_Click(object sender, RoutedEventArgs e)
    {
        LobbyService.WipeDatabase();
    }

    private void btnRegister_Click(object sender, RoutedEventArgs e)
    {
        _createAccount();
    }
    #endregion

    #region Bug Report UI
    private void btnSubmitBug_Click(object sender, RoutedEventArgs e)
    {
        tbBuggyReplayFileName.Text = FileRotator.GetLatestFileName("./Replays", string.Empty, ".sgs");
        tbBugMessage.Clear();
        submitBugWindow.Show();
    }

    private void btnSubmitBugCancel_Click(object sender, RoutedEventArgs e)
    {
        submitBugWindow.Close();
    }

    private void SubmitBugReport(ILobbyService service, Stream s, string message)
    {
        Stream upload = new MemoryStream();
        UnicodeEncoding uniEncoding = new UnicodeEncoding();
        if (s != null && s.Length > Misc.MaxBugReportSize) s = null;
        byte[] messageBytes = uniEncoding.GetBytes(message);
        byte[] intBytes = BitConverter.GetBytes(messageBytes.Length);
        upload.Write(intBytes, 0, intBytes.Length);
        upload.Write(messageBytes, 0, messageBytes.Length);
        if (s != null)
        {
            try
            {
                byte[] bytes = new byte[4];
                s.Read(bytes, 0, 4);
                int length = BitConverter.ToInt32(bytes, 0);
                s.Seek(length, SeekOrigin.Current);

                s.CopyTo(upload);
                s.Flush();
            }
            catch (Exception)
            {
                s = null;
            }
        }

        upload.Flush();
        upload.Seek(0, SeekOrigin.Begin);
        service.SubmitBugReport(upload);
    }

    private void btnSubmitBugConfirm_Click(object sender, RoutedEventArgs e)
    {
        FileStream fs = null;
        try
        {
            fs = new FileStream(tbBuggyReplayFileName.Text, FileMode.Open);
        }
        catch (Exception)
        {
        }
        if (fs != null && fs.Length > Misc.MaxBugReportSize - Misc.MaxBugMessgeSize - 1000)
        {
            fs = null;
        }

        string hostName = tab0HostName.Text;
        if (!hostName.Contains(":"))
        {
            hostName = hostName + ":" + DefaultLobbyPort;
        }

        try
        {
            // todo change to GRPC
            //var binding = new NetTcpBinding();
            //binding.Security.Mode = SecurityMode.None;
            //var endpoint = new EndpointAddress(string.Format("net.tcp://{0}/GameService", hostName));
            //var channelFactory = new DuplexChannelFactory<ILobbyService>(typeof(LobbyViewModel), binding, endpoint);
            //var server = channelFactory.CreateChannel();
            //SubmitBugReport(server, fs, tbBugMessage.Text);
            MessageBox.Show("Bug reported! Thanks for your participation.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot contact bug collection server! Error: " + ex.StackTrace);
        }
        submitBugWindow.Close();
    }

    private void btnPickReplayFile_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists("./Replays"))
        {
            Directory.CreateDirectory("./Replays");
        }

        OpenFileDialog dlg = new OpenFileDialog();
        dlg.InitialDirectory = Directory.GetCurrentDirectory() + "\\Replays";
        dlg.DefaultExt = ".sgs"; // Default file extension
        dlg.Filter = "Replay File (.sgs)|*.sgs|All Files (*.*)|*.*"; // Filter files by extension

        bool? result = dlg.ShowDialog();
        if (result == false) return;

        tbBuggyReplayFileName.Text = dlg.FileName;
    }
    #endregion

    public void Dispose()
    {
        if (_channelFactory != null)
        {
            try
            {
                _channelFactory.Close();
            }
            catch (Exception)
            {
                _channelFactory.Abort();
            }
            _channelFactory = null;
        }
    }
}
