using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using icpms_client.Common.Constants;
using icpms_client.Network.Constants;
using icpms_client.Network.Core;
using icpms_client.Network.Services.Home;
using icpms_client.Network.Services.Logs;
using icpms_client.Network.Services.Realtime;
using icpms_client.Network.Services.Vehicle;
using icpms_client.Network.Services.Visitor;
using icpms_client.Pages.Screens.Sections.ViewModels;
using icpms_client.Pages.Screens.ViewModels;
using icpms_client.Services.ExternalServices;
using icpms_client.Utils.Settings;
using icpms_client.Utils.UI.Theme;
using icpms_client.ViewModels.Main;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static IServiceProvider? ServiceProvider { get; set; }
    public static IConfiguration? Configuration { get; set; }

    public App()
    {
        XmlConfigurator.Configure();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        var services = new ServiceCollection();

        services.AddSingleton(Configuration);

        // Configuration binding
        services.Configure<ApiSettings>(options =>
            Configuration.GetSection("ApiSettings").Bind(options));
        services.Configure<EnvironmentSettings>(options =>
            Configuration.GetSection("EnvironmentSettings").Bind(options));
        services.Configure<ThemeSettings>(options =>
            Configuration.GetSection("ThemeSettings").Bind(options));
        services.Configure<Plugins>(options =>
            Configuration.GetSection("Plugins"));
        services.Configure<VehicleDetectionSettings>(options => 
            Configuration.GetSection("VehicleDetectionSettings").Bind(options));

        // Register services
        services.AddSingleton<IApiConstant, ApiConstant>();
        services.AddSingleton<ApiClient>();
        services.AddSingleton<IEnvironmentConstant, EnvironmentConstant>();
        services.AddSingleton<EnvironmentSettings>();
        services.AddSingleton<ThemeManager>();

        services.AddSingleton<IPluginConstants, PluginConstants>();
        services.AddSingleton<Plugins>();

        services.AddSingleton<VehicleDetectionRealtimeService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();

        services.AddSingleton<HomeScreenService>();
        services.AddSingleton<VehicleService>();
        services.AddSingleton<VisitorService>();
        
        services.AddSingleton<VehicleAlertRealtimeService>();
        services.AddSingleton<VehicleAlertLogService>();
        
        services.AddTransient<HomeScreenViewModel>();
        services.AddTransient<HomeParkingAreaSummaryViewModel>();
        services.AddTransient<HomeShiftSummaryViewModel>();
        services.AddTransient<RecentVisitorsSectionViewModel>();

        // Window
        services.AddTransient<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        ServiceProvider.GetRequiredService<ThemeManager>();

        ServiceProvider.GetRequiredService<VehicleDetectionRealtimeService>()
            .StartInBackground();
        
        ServiceProvider.GetRequiredService<VehicleAlertRealtimeService>()
            .StartInBackground();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (ServiceProvider?.GetService<VehicleDetectionRealtimeService>() is { } vehicleService)
        {
            await vehicleService.DisposeAsync();
        }
        if (ServiceProvider?.GetService<VehicleAlertRealtimeService>() is { } alertService)
        {
            await alertService.DisposeAsync();
        }
        base.OnExit(e);
    }

    /// <summary>
    /// Copies TS_DATA.db to a writable location if it doesn't exist.
    /// </summary>
    private static string PrepareWritableDatabase()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Flexitech", "Truckscale");

        Directory.CreateDirectory(appDataPath);

        string destinationDbPath = Path.Combine(appDataPath, "TS_DATA.db");

        if (!File.Exists(destinationDbPath))
        {
            string sourceDbPath = Path.Combine(AppContext.BaseDirectory, "TS_DATA.db");

            if (File.Exists(sourceDbPath))
            {
                File.Copy(sourceDbPath, destinationDbPath);
            }
            else
            {
                throw new FileNotFoundException("Bundled TS_DATA.db not found in install directory.", sourceDbPath);
            }
        }

        return destinationDbPath;
    }
}