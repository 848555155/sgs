using System.Windows.Media;

namespace Sanguosha.UI.Animations;

/// <summary>
/// Interaction logic for LoseHealthAnimation.xaml
/// </summary>
public partial class LoseHealthAnimation : FrameBasedAnimation
{
    private static readonly List<ImageSource> frames;

    static LoseHealthAnimation()
    {
        frames = LoadFrames("pack://application:,,,/Animations;component/LoseHealthAnimation", 18);
    }

    public LoseHealthAnimation()
    {

    }

    public override List<ImageSource> Frames => frames;

}
