using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

public class BoolToInvisibilityConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Visibility.Collapsed : (object)Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
