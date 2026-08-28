using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using icpms_client.Network.Request.Shift;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Auth;
using icpms_client.Network.Services.Auth;
using icpms_client.Network.Services.Shift;
using icpms_client.Network.Session;
using icpms_client.Services.UIServices;
using icpms_client.Utils.Storage;
using log4net;
using Newtonsoft.Json;

namespace icpms_client;

public partial class MainWindow
{
    private readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));
    private readonly AuthService _authService = new();
    private readonly ShiftService _shiftService = new();

    private bool _isLoading;

    public MainWindow()
    {
        MainWindowInstance = this;
        InitializeComponent();
        MainWindowFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
        MainFrame = MainWindowFrame;
        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        DialogService.ShowLoadingDialog(this, "Initializing...");
        await LoadUserAsync();
    }

    private async Task LoadUserAsync()
    {
        IsLoading = true;

        try
        {
            var session = await UserSessionStorage.LoadSessionAsync();
            if (session != null)
            {
                var response = await _authService.CheckToken(session.CurrentAuth?.AccessToken ?? "");
                if (response is { Success: true })
                {
                    var auth = (BaseResponse<AuthResponse>)response;
                    
                    if (auth.Data is { StartShift: false })
                    {
                        
                        Authenticated = true;
                        _log.Info("User is already logged in.");
                        UserSession.CurrentUser = session;
                        UserSession.CurrentShift = auth.Data!.ActiveShift!;

                        NavigateToMainPage();
                    }
                    else
                    {
                        _log.Warn("Token is valid but shift not start yet.");
                        InitializeLoginPage();
                        Authenticated = false;
                    }
                }
                else
                {
                    _log.Warn("Token validation failed. Redirecting to login.");
                    InitializeLoginPage();
                    Authenticated = false;
                }
            }
            else
            {
                _log.Warn("No user session found. Redirecting to login.");
                InitializeLoginPage();
                Authenticated = false;
            }
        }
        catch (Exception ex)
        {
            _log.Error($"Error loading user session: {ex.Message}, StackTrace: {ex.StackTrace}");
            InitializeLoginPage();
            Authenticated = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = (WindowState == WindowState.Maximized)
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        var result = DialogService.ShowWarningDialog("Are you sure you want to close the application?", MessageBoxButton.YesNoCancel);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                _log.Error($"Close() threw: {ex}");
            }
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
    }

    private void NotificationsButton_Click(object sender, RoutedEventArgs e)
    {
    }

    private void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = DialogService.ShowQuestionDialog("Do you want to end shift?", "Sign Out");
        
        if (result == MessageBoxResult.Yes)
        {
            DialogService.ShowStartShiftDialog(async void (data) =>
            {
                DialogService.HideStartShiftDialog();
                if (data != null)
                {
                    DialogService.ShowLoadingDialog(this, "Please wait...");
                    var request = new EndShiftRequest
                    {
                        ClosingCash = data.OpeningCash,
                        Remark = data.Remark
                    };
                    var response = await _shiftService.EndShift(request);

                    _log.Info($"EndShift response: {JsonConvert.SerializeObject(response)}");
                    
                    DialogService.HideLoading(this);
                    if (response is { Success: true })
                    {
                        UserSessionStorage.ClearSession();
                        Authenticated = false;
                        InitializeLoginPage();
                    }
                    else
                    {
                        DialogService.ShowErrorDialog("Failed to end shift!");
                    }
                    
                }
            }, "End Shift");
        }else if(result == MessageBoxResult.No){
            UserSessionStorage.ClearSession();
            Authenticated = false;
            InitializeLoginPage();
        }
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
    }

    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case 0x0024:
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
                break;
        }
        return IntPtr.Zero;
    }

    private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
        var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
        var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

        if (monitor != IntPtr.Zero)
        {
            var monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            GetMonitorInfo(monitor, ref monitorInfo);

            var workArea = monitorInfo.rcWork;
            var monitorArea = monitorInfo.rcMonitor;
            mmi.ptMaxPosition.x = Math.Abs(workArea.left - monitorArea.left);
            mmi.ptMaxPosition.y = Math.Abs(workArea.top - monitorArea.top);
            mmi.ptMaxSize.x = Math.Abs(workArea.right - workArea.left);
            mmi.ptMaxSize.y = Math.Abs(workArea.bottom - workArea.top);
        }

        Marshal.StructureToPtr(mmi, lParam, true);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x, y;
    }
}