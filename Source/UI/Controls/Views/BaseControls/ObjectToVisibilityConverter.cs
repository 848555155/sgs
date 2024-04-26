using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null || (value is string str && str == string.Empty) ? Visibility.Collapsed : (object)Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
