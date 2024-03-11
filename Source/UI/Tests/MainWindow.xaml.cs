using Sanguosha.Core.Games;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Tests;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        if (!PreloadCompleted) _Load();
        InitializeComponent();
    }

    private static readonly string[] _dictionaryNames = new string[] { "Cards.xaml", "Skills.xaml", "Game.xaml" };

    private void _LoadResources(string folderPath)
    {
        try
        {
            var files = Directory.GetFiles(string.Format("{0}/Texts", folderPath));
            foreach (var filePath in files)
            {
                if (!_dictionaryNames.Any(fileName => filePath.Contains(fileName))) continue;
                try
                {
                    Uri uri = new Uri(string.Format("pack://siteoforigin:,,,/{0}", filePath));
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources;component/Lobby.xaml") });
        }
        catch (DirectoryNotFoundException)
        {
        }
        PreloadCompleted = true;
    }

    private void _Load()
    {
        _LoadResources(ResourcesFolder);

        GameEngine.LoadExpansions(ExpansionFolder);

    }

    public static string ExpansionFolder = "./";
    public static string ResourcesFolder = "Resources";

    private static bool _preloadCompleted = false;

    internal static bool PreloadCompleted
    {
        get { return _preloadCompleted; }
        set
        {
            _preloadCompleted = value;
        }
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        PlayerViewTest window = new PlayerViewTest();
        window.Show();
    }
}
