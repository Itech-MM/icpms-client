using System.Windows;
using System.Windows.Media;
using icpms_client.UserControls.Customs.Forms.Validations.Form;

namespace icpms_client.Utils.UI.Theme;

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ThemeManager
{
    private readonly IConfigurationRoot? _config;
    private readonly string _configPath;
    public ThemeSettings CurrentTheme { get; private set; }

    public ThemeManager(IOptionsMonitor<ThemeSettings> themeOptions, IConfiguration config)
    {
        _config = config as IConfigurationRoot;

        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Flexitech", "ICPMS");

        Directory.CreateDirectory(appDataFolder);

        _configPath = Path.Combine(appDataFolder, "appsettings.user.json");

        if (!File.Exists(_configPath))
        {
            var defaultConfig = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(defaultConfig))
                File.Copy(defaultConfig, _configPath);
        }

        CurrentTheme = LoadThemeSettingsFromFile(_configPath) ?? themeOptions.CurrentValue;
        ApplyTheme(CurrentTheme.Mode == "Dark");

        themeOptions.OnChange(newTheme =>
        {
            if (newTheme.Mode == CurrentTheme.Mode) return;
            CurrentTheme = newTheme;
            ApplyTheme(newTheme.Mode == "Dark");
        });
    }

    private static ThemeSettings? LoadThemeSettingsFromFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var json = JObject.Parse(File.ReadAllText(path));
            return json["ThemeSettings"]?.ToObject<ThemeSettings>();
        }
        catch
        {
            return null;
        }
    }

    private void UpdateConfigFile(string newMode)
    {
        try
        {
            var json = File.Exists(_configPath)
                ? JObject.Parse(File.ReadAllText(_configPath))
                : new JObject();

            json["ThemeSettings"] ??= new JObject();
            json["ThemeSettings"]!["Mode"] = newMode;

            File.WriteAllText(_configPath, json.ToString(Formatting.Indented));
            _config?.Reload();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving theme: {ex.Message}");
        }
    }

    public void ToggleTheme(bool isDark)
    {
        var newMode = isDark ? "Dark" : "Light";
        if (CurrentTheme.Mode == newMode) return;

        UpdateConfigFile(newMode);
        CurrentTheme.Mode = newMode;
        ApplyTheme(isDark);
    }

    private void ApplyTheme(bool isDark)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ThemeHelper.SwitchTheme(isDark);
            _applyFormFields();
        });
    }

    private static void _applyFormFields()
    {
    }
}
