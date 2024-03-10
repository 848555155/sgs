using System.Windows.Media.Animation;

namespace Sanguosha.UI.Animations;

/// <summary>
/// Interaction logic for DamageAnimation.xaml
/// </summary>
public partial class DamageAnimation : AnimationBase
{
    public DamageAnimation()
    {
        InitializeComponent();
        mainAnimation = Resources["mainAnimation"] as Storyboard;
    }
}