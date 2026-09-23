using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using FlexiStream.Player;
using icpms_client.Common.Constants;
using icpms_client.Network.Constants;
using icpms_client.Network.Core;
using icpms_client.Network.Services.Gate;
using icpms_client.Network.Services.Home;
using icpms_client.Network.Services.Logs;
using icpms_client.Network.Services.Member;
using icpms_client.Network.Services.ParkingArea;
using icpms_client.Network.Services.Realtime;
using icpms_client.Network.Services.Shift;
using icpms_client.Network.Services.Vehicle;
using icpms_client.Network.Services.Visitor;
using icpms_client.Pages.Screens.Sections.ViewModels;
using icpms_client.Pages.Screens.ViewModels;
using icpms_client.Services.DeviceDiagnosis;
using icpms_client.Services.ExternalServices;
using icpms_client.State;
using icpms_client.Utils.Settings;
using icpms_client.Utils.UI.Theme;
using icpms_client.ViewModels.Main;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client;

public partial class App
{
    private const string MutexName = @"Global\ICPMS.Client.SingleInstance.7c1f4a9e";
    private const string ActivateEventName = @"Global\ICPMS.Client.Activate.7c1f4a9e";

    public static IServiceProvider? ServiceProvider { get; set; }
    public static IConfiguration? Configuration { get; set; }

    private CrashLogSettings _crashLogSettings = new();

    private Mutex? _mutex;
    private EventWaitHandle? _activateEvent;
    private bool _ownsMutex;

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            var earlyConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            earlyConfig.GetSection("CrashLogSettings").Bind(_crashLogSettings);
        }
        catch
        {
        }

        XmlConfigurator.Configure();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);
        _mutex = new Mutex(true, MutexName, out _ownsMutex);

        if (!_ownsMutex)
        {
            activateEvent.Set();
            activateEvent.Dispose();
            _mutex.Dispose();
            _mutex = null;
            Shutdown();
            return;
        }

        _activateEvent = activateEvent;
        StartActivationListener(activateEvent);

        base.OnStartup(e);

        try
        {
            PlayerBootstrapper.Initialize(Path.Combine(AppContext.BaseDirectory, "ffmpeg"));

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddSingleton(Configuration);

            services.Configure<ApiSettings>(options =>
                Configuration.GetSection("ApiSettings").Bind(options));
            services.Configure<EnvironmentSettings>(options =>
                Configuration.GetSection("EnvironmentSettings").Bind(options));
            services.Configure<ThemeSettings>(options =>
                Configuration.GetSection("ThemeSettings").Bind(options));
            services.Configure<Plugins>(options =>
                Configuration.GetSection("Plugins").Bind(options));
            services.Configure<VehicleDetectionSettings>(options =>
                Configuration.GetSection("VehicleDetectionSettings").Bind(options));
            services.Configure<CrashLogSettings>(options =>
                Configuration.GetSection("CrashLogSettings").Bind(options));

            services.AddSingleton<IApiConstant, ApiConstant>();
            services.AddSingleton<ApiClient>();
            services.AddSingleton<IEnvironmentConstant, EnvironmentConstant>();
            services.AddSingleton<EnvironmentSettings>();
            services.AddSingleton<ThemeManager>();

            services.AddSingleton<IPluginConstants, PluginConstants>();
            services.AddSingleton<Plugins>();

            services.AddSingleton<VehicleDetectionRealtimeService>();

            services.AddSingleton<MainViewModel>();

            services.AddSingleton<HomeScreenService>();
            services.AddSingleton<VehicleService>();
            services.AddSingleton<VisitorService>();
            services.AddSingleton<ParkingAreaService>();
            services.AddSingleton<ShiftService>();
            services.AddSingleton<MemberService>();
            services.AddSingleton<GateService>();

            services.AddSingleton<VehicleAlertRealtimeService>();
            services.AddSingleton<VehicleAlertLogService>();

            services.AddTransient<HomeScreenViewModel>();
            services.AddTransient<HomeParkingAreaSummaryViewModel>();
            services.AddTransient<HomeShiftSummaryViewModel>();
            services.AddTransient<RecentVisitorsSectionViewModel>();
            services.AddTransient<ParkingSessionSearchViewModel>();
            services.AddTransient<MemberScreenViewModel>();

            services.AddSingleton<IDeviceProbe, SimulatedDeviceProbe>();
            services.AddSingleton<IDeviceDiagnosisService, DeviceDiagnosisService>();
            services.AddTransient<DeviceDiagnosisViewModel>();

            services.AddSingleton<ShiftSummaryState>();

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
        catch (Exception ex)
        {
            LogFatal("OnStartup", ex);
            MessageBox.Show(
                $"The application failed to start:\n\n{ex.Message}\n\nSee the crash log for details.",
                "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (ServiceProvider?.GetService<VehicleDetectionRealtimeService>() is { } vehicleService)
            {
                await vehicleService.DisposeAsync();
            }
            if (ServiceProvider?.GetService<VehicleAlertRealtimeService>() is { } alertService)
            {
                await alertService.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            LogFatal("OnExit", ex);
        }

        _activateEvent?.Dispose();
        ReleaseInstanceMutex();

        base.OnExit(e);
    }

    private void StartActivationListener(EventWaitHandle activateEvent)
    {
        var listener = new Thread(() =>
        {
            try
            {
                while (activateEvent.WaitOne())
                    Dispatcher.InvokeAsync(ActivateMainWindow);
            }
            catch (ObjectDisposedException)
            {
            }
        })
        {
            IsBackground = true
        };

        listener.Start();
    }

    private void ActivateMainWindow()
    {
        var window = MainWindow;
        if (window == null) return;

        if (!window.IsVisible) window.Show();
        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;

        window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }

    private void ReleaseInstanceMutex()
    {
        if (_mutex == null) return;

        try
        {
            if (_ownsMutex) _mutex.ReleaseMutex();
        }
        catch (ApplicationException)
        {
        }

        _mutex.Dispose();
        _mutex = null;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogFatal("AppDomain.UnhandledException", e.ExceptionObject as Exception);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogFatal("Dispatcher.UnhandledException", e.Exception);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogFatal("TaskScheduler.UnobservedTaskException", e.Exception);
        e.SetObserved();
    }

    private void LogFatal(string source, Exception? ex)
    {
        try
        {
            var directory = string.IsNullOrWhiteSpace(_crashLogSettings.Directory)
                ? AppContext.BaseDirectory
                : Path.Combine(AppContext.BaseDirectory, _crashLogSettings.Directory);

            Directory.CreateDirectory(directory);

            var fileName = string.IsNullOrWhiteSpace(_crashLogSettings.FileName)
                ? "fatal-crash.log"
                : _crashLogSettings.FileName;

            var fullPath = Path.Combine(directory, fileName);

            File.AppendAllText(fullPath, $"{DateTime.Now:O} [{source}]\n{ex}\n\n");
        }
        catch
        {
        }
    }

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