using Sanguosha.UI.Animations;
using System.Windows.Media;

namespace Sanguosha.UI.Controls;

/// <summary>
/// Interaction logic for LoseHealthAnimation.xaml
/// </summary>
public class BigEquipBorder : FrameBasedAnimation
{
    private static readonly List<ImageSource> frames;

    static BigEquipBorder()
    {
        frames = LoadFrames("pack://application:,,,/Controls;component/Views/Resources/EquipButton/border", 10);
    }

    public BigEquipBorder()
    {
    }

    protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == VisibilityProperty)
        {
            IsActive = (Visibility == System.Windows.Visibility.Visible);
        }
    }

    public override List<ImageSource> Frames => frames;
}
