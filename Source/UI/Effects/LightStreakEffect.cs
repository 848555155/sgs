using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;


namespace Sanguosha.UI.Effects;

/// <summary>An effect that intensifies bright areas.</summary>
public class LightStreakEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(LightStreakEffect), 0);
    public static readonly DependencyProperty BrightThresholdProperty = DependencyProperty.Register("BrightThreshold", typeof(double), typeof(LightStreakEffect), new UIPropertyMetadata((double)0.5D, PixelShaderConstantCallback(0)));
    public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(LightStreakEffect), new UIPropertyMetadata((double)0.5D, PixelShaderConstantCallback(1)));
    public static readonly DependencyProperty AttenuationProperty = DependencyProperty.Register("Attenuation", typeof(double), typeof(LightStreakEffect), new UIPropertyMetadata((double)0.25D, PixelShaderConstantCallback(2)));
    public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(Vector), typeof(LightStreakEffect), new UIPropertyMetadata(new Vector(0.5D, 1D), PixelShaderConstantCallback(3)));
    public static readonly DependencyProperty InputSizeProperty = DependencyProperty.Register("InputSize", typeof(Size), typeof(LightStreakEffect), new UIPropertyMetadata(new Size(800D, 600D), PixelShaderConstantCallback(4)));
    private static readonly PixelShader pixelShader;

    static LightStreakEffect()
    {
        pixelShader = new PixelShader();
        pixelShader.UriSource = Global.MakePackUri("ShaderSource/LightStreak.ps");
    }

    public LightStreakEffect()
    {
        PixelShader = pixelShader;

        UpdateShaderValue(InputProperty);
        UpdateShaderValue(BrightThresholdProperty);
        UpdateShaderValue(ScaleProperty);
        UpdateShaderValue(AttenuationProperty);
        UpdateShaderValue(DirectionProperty);
        UpdateShaderValue(InputSizeProperty);
    }
    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }
    /// <summary>Threshold for selecting bright pixels.</summary>
    public double BrightThreshold
    {
        get => (double)GetValue(BrightThresholdProperty);
        set => SetValue(BrightThresholdProperty, value);
    }
    /// <summary>Contrast factor.</summary>
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    /// <summary>Attenuation factor.</summary>
    public double Attenuation
    {
        get => (double)GetValue(AttenuationProperty);
        set => SetValue(AttenuationProperty, value);
    }
    /// <summary>Direction of light streaks (as a vector).</summary>
    public Vector Direction
    {
        get => (Vector)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }
    /// <summary>Size of the input (in pixels).</summary>
    public Size InputSize
    {
        get => (Size)GetValue(InputSizeProperty);
        set => SetValue(InputSizeProperty, value);
    }
}

