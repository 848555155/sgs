using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls;

/// <summary>
/// Interaction logic for ChatWidget.xaml
/// </summary>
public partial class ChatWidget : UserControl
	{
		public ChatWidget()
		{
			this.InitializeComponent();
        _LoadFacials();
		}

    private void _LoadFacials()
    {
        var items = new ObservableCollection<int>();
        for (int i = 0; i < 100; i++)
        {
            string key = string.Format("Facial.{0}.Image", i);
            if (!Resources.Contains(key)) break;
            items.Add(i); 
        }
        gridFacials.DataContext = items;
    }

    private void btnFacial_Click(object sender, RoutedEventArgs e)
    {
        popFacials.IsOpen = !popFacials.IsOpen;
    }

    private void btnSmiley_Click(object sender, RoutedEventArgs e)
    {
        int index = (int)(sender as Button).DataContext;
        txtMessage.Text += "#" + index.ToString("D2");
        popFacials.IsOpen = false;
    }

    private void txtMessage_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && txtMessage.Text.Length > 0)
        {
            btnSend_Click(btnSend, new RoutedEventArgs());
        }
    }

    private void btnSend_Click(object sender, RoutedEventArgs e)
    {
        if (txtMessage.Text.Length > 0)
        {
            LobbyViewModel.Instance.SendMessage(txtMessage.Text);
            txtMessage.Text = string.Empty;
        }
    }
	}