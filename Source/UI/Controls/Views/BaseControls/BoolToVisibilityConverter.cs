using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

public class BoolToVisibilityConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool visible = (bool)value;
        if (visible)
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
