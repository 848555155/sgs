using Sanguosha.Lobby.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

/// <summary>
/// Interaction logic for SeatView.xaml
/// </summary>
public partial class SeatView : UserControl
{
    public SeatView()
    {
        this.LoadViewFromUri("/Controls;component/views/lobby/seatview.xaml");
        // Insert code required on object creation below this point.
    }

    public bool IsControllable
    {
        get { return (bool)GetValue(IsControllableProperty); }
        set { SetValue(IsControllableProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsControllable.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsControllableProperty =
        DependencyProperty.Register("IsControllable", typeof(bool), typeof(SeatView), new UIPropertyMetadata(OnControllableChanged));

    private static void OnControllableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var seat = d as SeatView;
        if (seat == null) return;
        if (seat.IsControllable)
        {
            seat.gridOpenSeat.Visibility = Visibility.Visible;
            seat.btnKick.Visibility = Visibility.Visible;
        }
        else
        {
            seat.gridOpenSeat.Visibility = Visibility.Collapsed;
            seat.btnKick.Visibility = Visibility.Collapsed;
        }
    }

}

public class SeatStateToControlStatusConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value == null || LobbyViewModel.Instance.CurrentSeat == null) return false;
        SeatState state = (SeatState)value;
        SeatState currentState = LobbyViewModel.Instance.CurrentSeat.State;
        return (currentState == SeatState.Host &&
                state != SeatState.Host && state != SeatState.Gaming);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}