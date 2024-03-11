using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sanguosha.UI.Controls;

public class NonClipGrid : Grid
{
    protected override Geometry GetLayoutClip(Size layoutSlotSize)
    {
        return ClipToBounds ? base.GetLayoutClip(layoutSlotSize) : null;
    }

}
