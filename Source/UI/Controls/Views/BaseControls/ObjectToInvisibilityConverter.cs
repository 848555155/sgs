using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace Sanguosha.UI.Controls;

public class ObjectToInvisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || (value is string && (value as string) == string.Empty))
        {
            return Visibility.Visible;
        }
        else
        {
            return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
