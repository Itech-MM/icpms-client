using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using icpms_client.Common.Enums;
using icpms_client.Common.Threads;
using icpms_client.Network.Session;
using icpms_client.Pages.Auth;
using icpms_client.Pages.Layout;
using icpms_client.Pages.Screens;
using icpms_client.Services.UIServices;
using log4net;
using Newtonsoft.Json;

namespace icpms_client.Common.UI;

public abstract class BaseWindow : Window, INotifyPropertyChanged
{
    private readonly ILog _log = LogManager.GetLogger(typeof(BaseWindow));
    public static BaseWindow MainWindowInstance = null!;
    public Frame MainFrame { get; set; } = null!;

    private PageLayout? _pageLayout;

    private bool _authenticated;
    private string _pageTitle = "Flexitech |";
    private string _terminalLabel = "ICPMS";
    private bool _hasNotifications;
    private int _notificationCount;

    private string _operatorName = string.Empty;
    private string _operatorRole = string.Empty;

    private AppPage _activeMenu = AppPage.Dashboard;

    private UserControl? _currentScreen;


    public void ApplyBlur(bool enable)
    {
        Effect = enable ? new BlurEffect { Radius = 10 } : null;
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            var comportFinder = ComportFinderThread.Instance;
            comportFinder.Dispose();
        }
        catch (Exception ex)
        {
            _log.Error($"Error on close {ex}");
        }
        base.OnClosed(e);
    }

    public bool Authenticated
    {
        get => _authenticated;
        set
        {
            _authenticated = value;
            OnPropertyChanged();
        }
    }


    public AppPage ActiveMenu
    {
        get => _activeMenu;
        set
        {
            _activeMenu = value;
            OnPropertyChanged();
        }
    }

    public string PageTitle
    {
        get => _pageTitle;
        set
        {
            _pageTitle = value;
            OnPropertyChanged();
        }
    }

    public string TerminalLabel
    {
        get => _terminalLabel;
        set
        {
            _terminalLabel = value;
            OnPropertyChanged();
        }
    }

    public bool HasNotifications
    {
        get => _hasNotifications;
        set
        {
            _hasNotifications = value;
            OnPropertyChanged();
        }
    }

    public int NotificationCount
    {
        get => _notificationCount;
        set
        {
            _notificationCount = value;
            OnPropertyChanged();
        }
    }

    public string OperatorName
    {
        get => _operatorName;
        set
        {
            _operatorName = value;
            OnPropertyChanged();
        }
    }

    public string OperatorRole
    {
        get => _operatorRole;
        set
        {
            _operatorRole = value;
            OnPropertyChanged();
        }
    }

    public void ApplyOperatorInfo()
    {
        var auth = UserSession.CurrentUser.CurrentAuth;
        Console.WriteLine($"Current auth :: {JsonConvert.SerializeObject(auth)}");
        OperatorName = auth?.Username ?? string.Empty;
        OperatorRole = auth?.Roles is { Count: > 0 } roles ? string.Join(", ", roles) : string.Empty;
    }

    public async Task InitializeLoginPageAsync()
    {
        DialogService.HideLoading(this);
        Authenticated = false;
        OperatorName = string.Empty;
        OperatorRole = string.Empty;

        if (_currentScreen != null)
        {
            var toDispose = _currentScreen;
            _currentScreen = null;
            await DetachAndDisposeOutgoingScreenAsync(toDispose);
        }
        _pageLayout = null;

        var loginPage = new LoginPage();
        TerminalLabel = "ICPMS";
        loginPage.OnAuthChanged += AuthChanged;
        MainFrame.Navigate(loginPage);
    }

    private async void AuthChanged(bool authenticated)
    {
        Authenticated = authenticated;
        if (authenticated)
        {
            ApplyOperatorInfo();
            await NavigateToMainPageAsync();
        }
    }

    public async Task NavigateToMainPageAsync()
    {
        DialogService.HideLoading(this);
        _pageLayout = new PageLayout();
        MainFrame.Navigate(_pageLayout);
        await ChangeScreenAsync(new HomeScreen(), terminalLabel: "Dashboard");
        ActiveMenu = AppPage.Dashboard;
    }

    public async Task ChangeScreenAsync(UserControl screen, string? title = null, string? terminalLabel = null)
    {
        if (title != null)
            PageTitle = title;

        if (terminalLabel != null)
            TerminalLabel = terminalLabel;

        if (_pageLayout != null)
        {
            var outgoing = _pageLayout.PushScreen(screen);
            await DetachAndDisposeOutgoingScreenAsync(outgoing);
        }
        else
        {
            MainFrame.Navigate(screen);
        }

        _currentScreen = screen;
    }

    private async Task DetachAndDisposeOutgoingScreenAsync(UserControl? outgoing)
    {
        if (outgoing == null)
            return;

        var dataContext = outgoing.DataContext;
        outgoing.DataContext = null;

        if (dataContext is IDisposableScreen navAware)
            navAware.OnNavigatedFrom();

        _pageLayout?.RemoveScreen(outgoing);

        if (dataContext is IAsyncDisposable asyncDisposableVm)
        {
            await SafeDisposeAsync(asyncDisposableVm);
        }
        else if (dataContext is IDisposable disposableVm)
        {
            disposableVm.Dispose();
        }

        if (outgoing is IDisposable disposableView)
        {
            try
            {
                disposableView.Dispose();
            }
            catch (Exception ex)
            {
                _log.Error($"Error disposing screen view: {ex}");
            }
        }
    }

    private async Task SafeDisposeAsync(IAsyncDisposable disposable)
    {
        try
        {
            await disposable.DisposeAsync();
        }
        catch (Exception ex)
        {
            _log.Error($"Error disposing screen view model async: {ex}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}