using System;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;

namespace Sanguosha.UI.Controls;

public class CardToolTipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        string convertType = parameter as string;
        if (convertType == null || value == null) return null;
        if (convertType == "Name" || convertType == "Description")
        {
            try
            {
                return Application.Current.Resources[string.Format("Card.{0}.{1}", value.ToString(), convertType)];
            }
            catch (Exception)
            {
                Trace.TraceWarning("Cannot find card {0}'s {1}", value.ToString(), convertType.ToLower());
            }
        }
        else if (convertType == "Usage")
        {
            string usage = Application.Current.TryFindResource(string.Format("Card.{0}.Usage", value.ToString())) as string;
            return usage;  
        }
        else if (convertType == "Suit")
        {
            return Application.Current.TryFindResource(string.Format("Card.Suit.{0}.SuitText", value.ToString(), convertType)) as string;               
        }
        else if (convertType == "SuitColor")
        {
            return Application.Current.TryFindResource(string.Format("Card.Suit.{0}.SuitBrush", value.ToString(), convertType)) as string;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
