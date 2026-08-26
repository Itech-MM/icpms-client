using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using icpms_client.Common.Threads;
using icpms_client.Pages.Auth;
using icpms_client.Services.UIServices;

namespace icpms_client.Common.UI;

public abstract class BaseWindow : Window, INotifyPropertyChanged
{
    public static BaseWindow MainWindowInstance = null!;
    public Frame MainFrame { get; set; } = null!;
    
    private bool _authenticated;
    
    public void ApplyBlur(bool enable)
    {
        Effect = enable ? new BlurEffect { Radius = 10 } : null;
    }
    protected override void OnClosed(EventArgs e)
    {
        Console.WriteLine("OnClosed: start");
        try
        {
            var comportFinder = ComportFinderThread.Instance;
            comportFinder.Dispose();
            Console.WriteLine("OnClosed: dispose succeeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnClosed: dispose threw: {ex}");
        }
        base.OnClosed(e);
        Console.WriteLine("OnClosed: end");
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
    
    /// <summary>
    /// Navigates to the Login Page.
    /// </summary>
    public void InitializeLoginPage()
    {
        DialogService.HideLoading(this);
        Authenticated = false;
        var loginPage = new LoginPage();
        loginPage.OnAuthChanged += AuthChanged;
        MainFrame.Navigate(loginPage);
    }
    
    /// <summary>
    /// Triggered when authentication status changes.
    /// </summary>
    private void AuthChanged(bool authenticated)
    {
        Authenticated = authenticated;
        if (authenticated)
        {
            NavigateToMainPage();
        }
    }

    /// <summary>
    /// Navigates to the Main Template page.
    /// </summary>
    public void NavigateToMainPage()
    {
        DialogService.HideLoading(this);
        /*MainFrame.Navigate(new MainTemplate());*/
    }

    public void ChangePage(UserControl page)
    {
        MainFrame.Navigate(page);
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}