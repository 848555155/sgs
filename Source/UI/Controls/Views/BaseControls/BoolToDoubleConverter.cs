using System.Globalization;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

public class BoolToDoubleConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? 1.0d : (object)0.0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
