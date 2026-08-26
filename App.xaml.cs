using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using icpms_client.Common.Constants;
using icpms_client.Network.Constants;
using icpms_client.Network.Core;
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

        /*string dbPath = PrepareWritableDatabase();
        string connectionString = $"Data Source={dbPath};Pooling=true;";*/

        /*services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));*/

        // Configuration binding
        services.Configure<ApiSettings>(options =>
            Configuration.GetSection("ApiSettings").Bind(options));
        services.Configure<EnvironmentSettings>(options =>
            Configuration.GetSection("EnvironmentSettings").Bind(options));
        services.Configure<ThemeSettings>(options =>
            Configuration.GetSection("ThemeSettings").Bind(options));
        services.Configure<Plugins>(options => 
            Configuration.GetSection("Plugins"));

        // Register services
        services.AddSingleton<IApiConstant, ApiConstant>();
        services.AddSingleton<ApiClient>();
        services.AddSingleton<IEnvironmentConstant, EnvironmentConstant>();
        services.AddSingleton<EnvironmentSettings>();
        services.AddSingleton<ThemeManager>();
        
        services.AddSingleton<IPluginConstants, PluginConstants>();
        services.AddSingleton<Plugins>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        
       /* services.AddSingleton<WeightTransactionTableViewModel>();
        services.AddSingleton<MainTemplateViewModel>();
        services.AddSingleton<DataSyncViewModel>();

        // Repositories and Services
        services.AddScoped<WeightTransactionRepository>();
        services.AddScoped<WeightTransactionService>();
        
        */

        // Window
        services.AddTransient<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        ServiceProvider.GetRequiredService<ThemeManager>();


        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
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