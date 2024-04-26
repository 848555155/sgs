using System.Windows;
using System.Windows.Controls;

namespace Sanguosha.UI.Controls;

public class SkillButtonTemplateSelector : DataTemplateSelector
{
    private static readonly ResourceDictionary dict;
    static SkillButtonTemplateSelector()
    {
        dict = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Controls;component/Views/Skills/SkillButtonStyles.xaml") };
    }
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        SkillCommand command = item as SkillCommand;
        if (command == null) return null;
        DataTemplate template = null;
        if (command is GuHuoSkillCommand)
        {
            template = dict["GuHuoButtonTemplate"] as DataTemplate;
        }
        else if (command.HeroName != null)
        {
            template = dict["RulerGivenSkillButtonTemplate"] as DataTemplate;
        }
        else
        {
            template = dict["SkillButtonTemplate"] as DataTemplate;
        }
        return template;
    }
}
