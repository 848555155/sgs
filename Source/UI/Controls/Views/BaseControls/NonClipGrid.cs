using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Sanguosha.UI.Controls;

public class NonClipGrid : Grid
{
    protected override Geometry GetLayoutClip(Size layoutSlotSize)
    {
        return ClipToBounds ? base.GetLayoutClip(layoutSlotSize) : null;
    }

}
