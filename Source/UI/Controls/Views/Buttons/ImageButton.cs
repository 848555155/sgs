using System.Windows;
using System.Windows.Controls;

namespace Sanguosha.UI.Controls;

public class ImageButton : Button
{
    static ImageButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton),
                                                 new FrameworkPropertyMetadata(typeof(ImageButton)));
    }

    #region properties

    public string HoverImage
    {
        get { return (string)GetValue(HoverImageProperty); }
        set { SetValue(HoverImageProperty, value); }
    }

    public string NormalImage
    {
        get { return (string)GetValue(NormalImageProperty); }
        set { SetValue(NormalImageProperty, value); }
    }

    public string PressedImage
    {
        get { return (string)GetValue(PressedImageProperty); }
        set { SetValue(PressedImageProperty, value); }
    }

    public string DisabledImage
    {
        get { return (string)GetValue(DisabledImageProperty); }
        set { SetValue(DisabledImageProperty, value); }
    }

    #endregion

    #region dependency properties

    public static readonly DependencyProperty DisabledImageProperty =
        DependencyProperty.Register(
            "DisabledImage", typeof(string), typeof(ImageButton));

    public static readonly DependencyProperty HoverImageProperty =
        DependencyProperty.Register(
            "HoverImage", typeof(string), typeof(ImageButton));

    public static readonly DependencyProperty NormalImageProperty =
        DependencyProperty.Register(
            "NormalImage", typeof(string), typeof(ImageButton));

    public static readonly DependencyProperty PressedImageProperty =
        DependencyProperty.Register(
            "PressedImage", typeof(string), typeof(ImageButton));

    #endregion
}
