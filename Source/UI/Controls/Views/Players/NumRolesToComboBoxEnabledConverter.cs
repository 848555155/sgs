using Sanguosha.Core.Games;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

public class NumRolesToComboBoxEnabledConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        List<Role> roles = value as List<Role>;
        return (roles != null) && (roles.Count > 1);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
