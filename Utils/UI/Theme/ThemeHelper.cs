using System.Windows;

namespace icpms_client.Utils.UI.Theme;

public class ThemeHelper
{
    public static void SwitchTheme(bool isDark)
    {
        var app = Application.Current;
        var dictionaries = app.Resources.MergedDictionaries;

        // Clear ALL theme dictionaries (important!)
        var themesToRemove = dictionaries
            .Where(d => d.Source?.ToString().Contains("Styles/") == true)
            .ToList();

        foreach(var theme in themesToRemove)
        {
            dictionaries.Remove(theme);
        }

        // Load new theme
        var themeUri = new Uri(isDark 
            ? "pack://application:,,,/Assets/Styles/DarkTheme.xaml"
            : "pack://application:,,,/Assets/Styles/LightTheme.xaml");

        dictionaries.Add(new ResourceDictionary { Source = themeUri });

        // Force UI refresh
        InvalidateVisuals(app.MainWindow);
    }

    private static void InvalidateVisuals(Window? window)
    {
        if (window == null) return;
    
        var oldContent = window.Content;
        window.Content = null;
        window.Content = oldContent;
    }
}