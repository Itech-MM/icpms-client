using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using icpms_client.Utils.UI.Theme;

namespace icpms_client.UserControls.UI.Reusable.Buttons
{
    public partial class TextButton : UserControl, INotifyPropertyChanged
    {
        private static readonly RoutedEvent ButtonClickEvent = EventManager.RegisterRoutedEvent("ButtonClick",
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextButton));
        
            public static readonly DependencyProperty TextProperty =
                DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextButton));
        
            public static readonly DependencyProperty BackgroundColorProperty =
                DependencyProperty.Register(nameof(BackgroundColor), typeof(Brush), typeof(TextButton),
                    new PropertyMetadata(new SolidColorBrush(ColorConverter.ConvertFromString("#F4F4F4") as Color? ??
                                                             Colors.Transparent)));
        
            public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(nameof(TextColor),
                typeof(Brush), typeof(TextButton), new PropertyMetadata(Brushes.Black));

            public static readonly DependencyProperty MaxButtonHeightProperty =
                DependencyProperty.Register(nameof(MaxButtonHeight), typeof(double), typeof(TextButton), new PropertyMetadata(AppDimension.MaxButtonHeight));

            public static readonly DependencyProperty ButtonHeightProperty =
                DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(TextButton),
                    new PropertyMetadata(AppDimension.ButtonHeight));
            public string Text
            {
                get => (string)GetValue(TextProperty);
                set
                {
                    SetValue(TextProperty, value);
                    OnPropertyChanged(nameof(Text));
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

            public double MaxButtonHeight
            {
                get => (double)GetValue(MaxButtonHeightProperty);
                set
                {
                    SetValue(MaxButtonHeightProperty, value);
                    OnPropertyChanged(nameof(MaxButtonHeight));
                }
            }
            
            public double ButtonHeight
            {
                get => (double)GetValue(ButtonHeightProperty);
                set
                {
                    SetValue(ButtonHeightProperty, value);
                    OnPropertyChanged(nameof(ButtonHeight));
                }
            }
            
        public TextButton()
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
}
