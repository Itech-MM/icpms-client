using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using icpms_client.Common.Enums;
using icpms_client.Common.Threads;
using icpms_client.Pages.Auth;
using icpms_client.Pages.Layout;
using icpms_client.Pages.Screens;
using icpms_client.Services.UIServices;
using log4net;

namespace icpms_client.Common.UI;

public abstract class BaseWindow : Window, INotifyPropertyChanged
{
    private readonly ILog _log = LogManager.GetLogger(typeof(BaseWindow));
    public static BaseWindow MainWindowInstance = null!;
    public Frame MainFrame { get; set; } = null!;

    private PageLayout? _pageLayout;

    private bool _authenticated;
    private string _pageTitle = "Flexitech | ";
    private string _terminalLabel = "ICPMS";
    private bool _hasNotifications;
    private int _notificationCount;
    
    private AppPage _activeMenu = AppPage.Dashboard;


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

    public void InitializeLoginPage()
    {
        DialogService.HideLoading(this);
        Authenticated = false;
        _pageLayout = null;
        var loginPage = new LoginPage();
        loginPage.OnAuthChanged += AuthChanged;
        MainFrame.Navigate(loginPage);
    }

    private void AuthChanged(bool authenticated)
    {
        Authenticated = authenticated;
        if (authenticated)
        {
            NavigateToMainPage();
        }
    }

    public void NavigateToMainPage()
    {
        DialogService.HideLoading(this);
        _pageLayout = new PageLayout();
        MainFrame.Navigate(_pageLayout);
        ChangeScreen(new HomeScreen(), terminalLabel: "Dashboard");
        ActiveMenu = AppPage.Dashboard;
    }

    public void ChangeScreen(UserControl screen, string? title = null, string? terminalLabel = null)
    {
        if (title != null)
            PageTitle = title;

        if (terminalLabel != null)
            TerminalLabel = terminalLabel;

        if (_pageLayout != null)
        {
            _pageLayout.PageContent = screen;
        }
        else
        {
            MainFrame.Navigate(screen);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}