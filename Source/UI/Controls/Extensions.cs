using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows;

namespace Sanguosha.UI.Controls;

internal static class Extension
{
    public static void LoadViewFromUri(this FrameworkElement userControl, string baseUri)
    {
        try { 
            var resourceLocater = new Uri(baseUri, UriKind.Relative); 
            var exprCa = (PackagePart)typeof(Application).GetMethod("GetResourceOrContentPart", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [resourceLocater]); 
            var stream = exprCa.GetStream(); 
            var uri = new Uri((Uri)typeof(BaseUriHelper).GetProperty("PackAppBaseUri", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null), resourceLocater); 
            var parserContext = new ParserContext { BaseUri = uri }; typeof(XamlReader).GetMethod("LoadBaml", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, [stream, parserContext, userControl, true]); }
        catch (Exception)
        { //log
        }
    }
}