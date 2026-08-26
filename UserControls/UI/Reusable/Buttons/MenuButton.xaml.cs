using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using log4net;

namespace icpms_client.UserControls.UI.Reusable.Buttons;

public partial class MenuButton : UserControl, INotifyPropertyChanged
{
    private static readonly RoutedEvent ButtonClickEvent = EventManager.RegisterRoutedEvent("ButtonClick",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuButton));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(MenuButton));

    public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register(nameof(BackgroundColor), typeof(Brush), typeof(MenuButton),
            new PropertyMetadata(new SolidColorBrush(ColorConverter.ConvertFromString("#F4F4F4") as Color? ??
                                                     Colors.Transparent)));

    public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(nameof(TextColor),
        typeof(Brush), typeof(MenuButton), new PropertyMetadata(Brushes.Black));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(MenuButton), new PropertyMetadata(false));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            OnPropertyChanged(nameof(Text));
        }
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set
        {
            SetValue(IsActiveProperty, value);
            OnPropertyChanged(nameof(IsActive));
        }
    }
    
    public Brush BackgroundColor
    {
        get => (Brush)GetValue(BackgroundColorProperty);
        set
        {
            SetValue(BackgroundColorProperty, value);
            OnPropertyChanged(nameof(BackgroundColor));
        }
    }

    public Brush TextColor
    {
        get => (Brush)GetValue(TextColorProperty);
        set
        {
            SetValue(TextColorProperty, value);
            OnPropertyChanged(nameof(TextColor));
        }
    }

    public event RoutedEventHandler ButtonClick
    {
        add => AddHandler(ButtonClickEvent, value);
        remove => RemoveHandler(ButtonClickEvent, value);
    }

    public MenuButton()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ButtonClickEvent, this));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}