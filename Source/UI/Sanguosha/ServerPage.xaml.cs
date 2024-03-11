using Sanguosha.Lobby.Server;
using System.Windows;
using System.Windows.Controls;

namespace Sanguosha.UI.Main;

/// <summary>
/// Interaction logic for ServerPage.xaml
/// </summary>
public partial class ServerPage : Page
{
    public ServerPage()
    {
        InitializeComponent();
    }

    private void btnGoBack_Click(object sender, RoutedEventArgs e)
    {
        //Host.CloseAsync();
        this.NavigationService.Navigate(Login.Instance);
    }

    //public ServiceHostBase Host { get; set; }

    private void cbAllowCheating_Click_1(object sender, RoutedEventArgs e)
    {
        LobbyService.CheatEnabled = cbAllowCheating.IsChecked == true;
    }

    public LobbyService GameService { get; set; }
}
