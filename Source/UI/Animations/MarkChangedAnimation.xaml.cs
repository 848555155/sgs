using System.Windows.Media;

namespace Sanguosha.UI.Animations;

/// <summary>
/// Interaction logic for LoseHealthAnimation.xaml
/// </summary>
public partial class MarkChangedAnimation : FrameBasedAnimation
{
    private static readonly List<ImageSource> frames;

    static MarkChangedAnimation()
    {
        frames = LoadFrames("pack://application:,,,/Animations;component/MarkChangedAnimation", 7);
    }

    public MarkChangedAnimation()
    {

    }

    public override List<ImageSource> Frames => frames;

}
