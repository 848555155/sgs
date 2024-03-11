using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Sanguosha.UI.Main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if !DEBUG
        automaticUpdater.ForceCheckForUpdate();
        automaticUpdater.ReadyToBeInstalled += (o, e) => { automaticUpdater.InstallNow(); };
#endif
        Controls.LobbyView.GlobalNavigationService = this.MainFrame.NavigationService;
        this.MainFrame.NavigationService.Navigated += NavigationService_Navigated;
        MainFrame.Navigate(Login.Instance);
    }

    private void NavigationService_Navigated(object sender, NavigationEventArgs e)
    {
        this.MainFrame.NavigationService.RemoveBackEntry();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        // MainFrame.Navigate(new Uri("pack://application:,,,/Sanguosha;component/MainGame.xaml"));
    }
}
