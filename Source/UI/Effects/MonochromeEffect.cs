namespace Sanguosha.UI.Effects;

// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
#if SILVERLIGHT 
using UIPropertyMetadata = System.Windows.PropertyMetadata ;      
#endif
/// <summary>
/// This is the implementation of an extensible framework ShaderEffect which loads
/// a shader model 2 pixel shader. Dependecy properties declared in this class are mapped
/// to registers as defined in the *.ps file being loaded below.
/// </summary>
public class MonochromeEffect : ShaderEffect
{
    #region Dependency Properties

    /// <summary>
    /// Gets or sets the FilterColor variable within the shader.
    /// </summary>
    public static readonly DependencyProperty FilterColorProperty = DependencyProperty.Register("FilterColor", typeof(Color), typeof(MonochromeEffect), new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

    /// <summary>
    /// Gets or sets the Strength variable within the shader.
    /// </summary>
    public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register("Strength", typeof(double), typeof(MonochromeEffect), new UIPropertyMetadata(1.0d, PixelShaderConstantCallback(1), CoerceStrength));

    /// <summary>
    /// Gets or sets the Input of the shader.
    /// </summary>
    public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(MonochromeEffect), 0);

    #endregion

    #region Constructors

    /// <summary>
    /// Creates an instance and updates the shader's variables to the default values.
    /// </summary>
    public MonochromeEffect()
    {
        var pixelShader = new PixelShader
        {
            UriSource = Global.MakePackUri("ShaderSource/Monochrome.ps")
        };
        PixelShader = pixelShader;

        UpdateShaderValue(InputProperty);
        UpdateShaderValue(FilterColorProperty);
        UpdateShaderValue(StrengthProperty);
    }

    #endregion

    /// <summary>
    /// Gets or sets the FilterColor variable within the shader.
    /// </summary>
    public Color FilterColor
    {
        get => (Color)GetValue(FilterColorProperty);
        set => SetValue(FilterColorProperty, value);
    }

    public double Strength
    {
        get => (double)GetValue(StrengthProperty);
        set => SetValue(StrengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the input used in the shader.
    /// </summary>
    [System.ComponentModel.BrowsableAttribute(false)]
    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    private static object CoerceStrength(DependencyObject d, object value)
    {
        var effect = (MonochromeEffect)d;
        var newStrength = (double)value;
        return newStrength is < 0.0 or > 1.0 ? effect.Strength : newStrength;
    }
}
