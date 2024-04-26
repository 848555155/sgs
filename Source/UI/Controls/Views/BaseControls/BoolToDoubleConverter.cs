using System;
using System.Globalization;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

public class BoolToDoubleConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool visible = (bool)value;
        if (visible)
        {
            return 1.0d;
        }
        else
        {
            return 0.0d;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
