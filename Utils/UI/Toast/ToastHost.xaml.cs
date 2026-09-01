using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using icpms_client.Common.UI;

namespace icpms_client.Utils.UI.Toast;

public partial class ToastHost
{
    private const int WsExNoactivate = 0x08000000;
    private const int WsExToolwindow = 0x00000080;
    private const int GwlExstyle = -20;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    private readonly BaseWindow? _owner;
    private readonly ObservableCollection<ToastNotification> _toasts = new();

    private ToastPosition _position;

    public ToastHost(BaseWindow? owner, ToastPosition position = ToastPosition.TopCenter)
    {
        InitializeComponent();

        _owner = owner;
        _position = position;
        ToastItemsControl.ItemsSource = _toasts;

        if (_owner != null)
        {
            _owner.LocationChanged += (_, _) => RepositionToOwner();
            _owner.SizeChanged += (_, _) => RepositionToOwner();
            _owner.Closed += (_, _) => Close();
        }

        SizeChanged += (_, _) => RepositionToOwner();
        Loaded += (_, _) => ApplyPosition();
    }

    public void AddToast(ToastType type, string message, string? title, TimeSpan duration)
    {
        var toast = new ToastNotification(type, message, title, duration);
        toast.RequestClose += OnToastRequestClose;

        var isTop = _position is ToastPosition.TopLeft or ToastPosition.TopCenter or ToastPosition.TopRight;
        if (isTop)
            _toasts.Insert(0, toast);
        else
            _toasts.Add(toast);

        if (!IsVisible)
        {
            ApplyPosition();
            Show();
        }
    }

    private void ApplyPosition()
    {
        const double edge = 16;

        ToastItemsControl.Margin = _position switch
        {
            ToastPosition.TopLeft      => new Thickness(edge, edge, 0, 0),
            ToastPosition.TopCenter    => new Thickness(0, edge, 0, 0),
            ToastPosition.TopRight     => new Thickness(0, edge, edge, 0),
            ToastPosition.BottomLeft   => new Thickness(edge, 0, 0, edge),
            ToastPosition.BottomCenter => new Thickness(0, 0, 0, edge),
            ToastPosition.BottomRight  => new Thickness(0, 0, edge, edge),
            _ => new Thickness(0, 0, edge, edge)
        };

        var panel = FindVisualChild<StackPanel>(ToastItemsControl);
        if (panel != null)
        {
            panel.VerticalAlignment = _position is ToastPosition.TopLeft or ToastPosition.TopCenter or ToastPosition.TopRight
                ? VerticalAlignment.Top
                : VerticalAlignment.Bottom;

            panel.HorizontalAlignment = _position switch
            {
                ToastPosition.TopLeft or ToastPosition.BottomLeft => HorizontalAlignment.Left,
                ToastPosition.TopCenter or ToastPosition.BottomCenter => HorizontalAlignment.Center,
                _ => HorizontalAlignment.Right
            };
        }

        RepositionToOwner();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = GetWindowLong(hwnd, GwlExstyle);
        SetWindowLong(hwnd, GwlExstyle, exStyle | WsExNoactivate | WsExToolwindow);
    }

    private void OnToastRequestClose(ToastNotification toast)
    {
        Dispatcher.Invoke(() =>
        {
            toast.RequestClose -= OnToastRequestClose;

            var border = ToastItemsControl.ItemContainerGenerator.ContainerFromItem(toast) is ContentPresenter container ? FindToastBorder(container) : null;

            if (border == null)
            {
                _toasts.Remove(toast);
                HideIfEmpty();
                return;
            }

            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(160));
            var slideOut = new DoubleAnimation
            {
                To = 40,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var sb = new Storyboard();
            Storyboard.SetTarget(fadeOut, border);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
            sb.Children.Add(fadeOut);

            Storyboard.SetTarget(slideOut, border);
            Storyboard.SetTargetProperty(slideOut, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            sb.Children.Add(slideOut);

            sb.Completed += (_, _) =>
            {
                _toasts.Remove(toast);
                HideIfEmpty();
            };
            sb.Begin();
        });
    }

    private void HideIfEmpty()
    {
        if (_toasts.Count == 0)
        {
            Hide();
        }
    }

    private static FrameworkElement? FindToastBorder(DependencyObject container)
    {
        if (VisualTreeHelper.GetChildrenCount(container) == 0) return null;
        return VisualTreeHelper.GetChild(container, 0) as FrameworkElement;
    }

    private void Toast_Loaded(object sender, RoutedEventArgs e)
    {
        var border = (Border)sender;

        UpdateClip(border);
        border.SizeChanged += (s, args) => UpdateClip(border);

        if (sender is not Border { DataContext: ToastNotification toast } root) return;
        if (!toast.ShowProgress) return;

        if (root.FindName("ProgressTrack") is not Border track ||
            root.FindName("ProgressFill") is not Border fill) return;

        root.Dispatcher.InvokeAsync(() =>
        {
            fill.Width = track.ActualWidth;
            var anim = new DoubleAnimation
            {
                From = track.ActualWidth,
                To = 0,
                Duration = toast.Duration,
                FillBehavior = FillBehavior.HoldEnd
            };
            fill.BeginAnimation(FrameworkElement.WidthProperty, anim);
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private static void UpdateClip(Border border)
    {
        border.Clip = new RectangleGeometry
        {
            RadiusX = border.CornerRadius.TopLeft,
            RadiusY = border.CornerRadius.TopLeft,
            Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight)
        };
    }

    public ToastPosition Position
    {
        get => _position;
        set
        {
            if (_position == value) return;
            _position = value;
            ApplyPosition();
        }
    }

    private void RepositionToOwner()
    {
        if (_owner == null) return;

        Left = _position switch
        {
            ToastPosition.TopLeft or ToastPosition.BottomLeft => _owner.Left,
            ToastPosition.TopCenter or ToastPosition.BottomCenter =>
                _owner.Left + (_owner.ActualWidth - ActualWidth) / 2,
            _ => _owner.Left + _owner.ActualWidth - ActualWidth
        };

        Top = _position is ToastPosition.TopLeft or ToastPosition.TopCenter or ToastPosition.TopRight
            ? _owner.Top
            : _owner.Top + _owner.ActualHeight - ActualHeight;

        MaxHeight = _owner.ActualHeight;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) return typed;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }
}