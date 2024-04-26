using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;


namespace Sanguosha.UI.Effects;

/// <summary>An effect that blends between partial desaturation and a two-color ramp.</summary>
public class RippleTransitionEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(RippleTransitionEffect), 0);
    public static readonly DependencyProperty Texture2Property = RegisterPixelShaderSamplerProperty("Texture2", typeof(RippleTransitionEffect), 1);
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(RippleTransitionEffect), new UIPropertyMetadata((double)30D, PixelShaderConstantCallback(0)));
    public RippleTransitionEffect()
    {
        var pixelShader = new PixelShader();
        pixelShader.UriSource = Global.MakePackUri("ShaderSource/RippleTransition.ps");
        PixelShader = pixelShader;

        UpdateShaderValue(InputProperty);
        UpdateShaderValue(Texture2Property);
        UpdateShaderValue(ProgressProperty);
    }
    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }
    public Brush Texture2
    {
        get => (Brush)GetValue(Texture2Property);
        set => SetValue(Texture2Property, value);
    }
    /// <summary>The amount(%) of the transition from first texture to the second texture. </summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
}
