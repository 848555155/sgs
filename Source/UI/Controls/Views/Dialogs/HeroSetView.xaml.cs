using Sanguosha.Core.Cards;
using System.Windows;
using System.Windows.Controls;

namespace Sanguosha.UI.Controls;


public delegate void SkillNameSelectedHandler(string skillName);

/// <summary>
/// Interaction logic for HeroSetView.xaml
/// </summary>
public partial class HeroSetView : UserControl
{
    public HeroSetView()
    {
        this.InitializeComponent();
    }

    private void gridHeroSet_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // TODO: Add event handler implementation here.
        var model = gridHeroSet.SelectedItem as HeroViewModel;
        if (model != null)
        {
            heroCardView.DataContext = new CardViewModel()
            {
                Card = new Card()
                {
                    Id = model.Id
                }
            };
            heroCardView.Visibility = Visibility.Visible;
            gridHeroInfo.Visibility = Visibility.Visible;
        }
        else
        {
            heroCardView.Visibility = Visibility.Collapsed;
            gridHeroInfo.Visibility = Visibility.Collapsed;
        }
    }

    public event SkillNameSelectedHandler OnSkillNameSelected;

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        SkillNameSelectedHandler handle = OnSkillNameSelected;
        if (handle != null)
        {
            string skillName = (sender as Button).DataContext as string;
            if (skillName != null && skillName != string.Empty)
            {
                handle(skillName);
            }
        }
    }
}