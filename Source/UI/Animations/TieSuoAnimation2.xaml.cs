using System.Collections.Generic;
using System.Windows.Media;

namespace Sanguosha.UI.Animations;

/// <summary>
/// Interaction logic for TieSuoAnimation2.xaml
/// </summary>
public partial class TieSuoAnimation2 : FrameBasedAnimation
{
    public TieSuoAnimation2()
    {
        InitializeComponent();
    }

    private static readonly List<ImageSource> frames;

    static TieSuoAnimation2()
    {
        frames = LoadFrames("pack://application:,,,/Animations;component/TieSuoAnimation2", 11);
    }

    public override List<ImageSource> Frames
    {
        get
        {
            return frames;
        }
    }
}
