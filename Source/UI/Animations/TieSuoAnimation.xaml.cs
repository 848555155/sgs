using System.Windows.Media;

namespace Sanguosha.UI.Animations;

/// <summary>
/// Interaction logic for TieSuoAnimation.xaml
/// </summary>
public partial class TieSuoAnimation : FrameBasedAnimation
{
    public TieSuoAnimation()
    {
        InitializeComponent();
    }

    private static readonly List<ImageSource> frames;

    static TieSuoAnimation()
    {
        frames = LoadFrames("pack://application:,,,/Animations;component/TieSuoAnimation", 11);
    }

    public override List<ImageSource> Frames => frames;
}
