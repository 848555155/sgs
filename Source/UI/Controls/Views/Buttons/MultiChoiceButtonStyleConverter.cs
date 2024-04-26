using Sanguosha.Core.UI;
using System.Windows;
using System.Windows.Data;

namespace Sanguosha.UI.Controls;

public class MultiChoiceButtonStyleConverter : IValueConverter
{
    static MultiChoiceButtonStyleConverter()
    {
        dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Controls;component/Views/Buttons/MultiChoiceButton.xaml")
        };
    }

    private static readonly ResourceDictionary dict;
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not OptionPrompt choiceKey) return null;
        if (Prompt.SuitChoices.Contains(choiceKey) || Prompt.AllegianceChoices.Contains(choiceKey))
        {
            return dict["MultiChoiceCustomButtonStyle"] as Style;
        }
        else
        {
            return dict["MultiChoiceButtonStyle"] as Style;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MultiChoiceKeyConverter : IValueConverter
{
    static MultiChoiceKeyConverter()
    {
        dict = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Controls;component/Views/Buttons/MultiChoiceButton.xaml")
        };
    }

    private static readonly ResourceDictionary dict;
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        OptionPrompt choiceKey = value as OptionPrompt;
        if (choiceKey == null) return null;
        return LogFormatter.Translate(choiceKey);
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
