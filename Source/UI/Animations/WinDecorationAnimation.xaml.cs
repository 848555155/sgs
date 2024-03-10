using System.Collections.Generic;
using System.Windows.Media;

namespace Sanguosha.UI.Animations;

/// <summary>
/// Interaction logic for LoseHealthAnimation.xaml
/// </summary>
public partial class WinDecorationAnimation : FrameBasedAnimation
{
    private static readonly List<ImageSource> frames;

    static WinDecorationAnimation()
    {
        frames = LoadFrames("pack://application:,,,/Animations;component/WinAnimation", 20);
    }

    public WinDecorationAnimation()
    {
        WrapAround = true;
        FramesPerSecond = 30;
        Start();
    }

    public override List<ImageSource> Frames
    {
        get
        {
            return frames;
        }
    }

}
