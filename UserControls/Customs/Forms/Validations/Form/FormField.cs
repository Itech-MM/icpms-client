using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using icpms_client.UserControls.UI.Reusable.Inputs.Formatter;

namespace icpms_client.UserControls.Customs.Forms.Validations.Form
{
    public abstract class FormField : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(FormField),
                new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ValidationRuleProperty =
            DependencyProperty.Register(nameof(ValidationRule), typeof(ITextValidationRule), typeof(FormField),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(FormField),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register(nameof(BackgroundColor), typeof(Brush), typeof(FormField),
                new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty RoundedRadiusProperty =
            DependencyProperty.Register(nameof(RoundedRadius), typeof(double), typeof(FormField),
                new PropertyMetadata(10.0));

        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register(nameof(PlaceHolder), typeof(string), typeof(FormField),
                new PropertyMetadata("Enter here"));

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Brush), typeof(FormField),
                new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty BorderSizeProperty =
            DependencyProperty.Register(nameof(BorderSize), typeof(double), typeof(FormField),
                new PropertyMetadata(2.0));

        public static readonly DependencyProperty ContentPaddingProperty =
            DependencyProperty.Register(nameof(ContentPadding), typeof(Thickness), typeof(FormField),
                new PropertyMetadata(new Thickness(2, 2, 2, 2)));

        public static readonly DependencyProperty ShowErrorMessageProperty =
            DependencyProperty.Register(nameof(ShowErrorMessage), typeof(bool), typeof(FormField),
                new PropertyMetadata(false));

        public static readonly DependencyProperty FocusColorProperty =
            DependencyProperty.Register(nameof(FocusColor), typeof(Brush), typeof(FormField),
                new PropertyMetadata(new BrushConverter().ConvertFrom("#80BDFF")));
        
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(nameof(BorderColor), typeof(Brush), typeof(FormField),
                new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(FormField), new PropertyMetadata(""));

        public static readonly DependencyProperty ReadonlyProperty =
            DependencyProperty.Register(nameof(Readonly), typeof(bool), typeof(FormField), new PropertyMetadata(false));

        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.Register(nameof(Enable), typeof(bool), typeof(FormField), new PropertyMetadata(true));


        public static readonly DependencyProperty FormattingTypeProperty =
            DependencyProperty.Register(nameof(TextFormatting), typeof(FormattingType), typeof(FormField),
                new PropertyMetadata(FormattingType.Text));

        public static readonly DependencyProperty LengthProperty =
            DependencyProperty.Register(nameof(Length), typeof(int), typeof(FormField), new PropertyMetadata(0));

        public static readonly DependencyProperty FormTextAlignmentProperty =
            DependencyProperty.Register(nameof(FormTextAlignment), typeof(TextAlignment), typeof(FormField),
                new PropertyMetadata(TextAlignment.Left));

        public static readonly DependencyProperty UseVirtualKeyboardProperty =
            DependencyProperty.Register(nameof(UseVirtualKeyboard), typeof(bool), typeof(FormField),
                new PropertyMetadata(true));
        
        public static readonly DependencyProperty IsErrorProperty =
            DependencyProperty.Register(nameof(IsError), typeof(bool), typeof(FormField), 
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsInputFocusProperty =
            DependencyProperty.Register(nameof(IsInputFocus), typeof(bool), typeof(FormField), 
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty InputHeightProperty =
            DependencyProperty.Register(nameof(InputHeight), typeof(double), typeof(FormField), 
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool UseVirtualKeyboard
        {
            get => (bool)GetValue(UseVirtualKeyboardProperty);
            set => SetValue(UseVirtualKeyboardProperty, value);
        }

        public TextAlignment FormTextAlignment
        {
            get => (TextAlignment)GetValue(FormTextAlignmentProperty);
            set => SetValue(FormTextAlignmentProperty, value);
        }

        public FormattingType TextFormatting
        {
            get => (FormattingType)GetValue(FormattingTypeProperty);
            set => SetValue(FormattingTypeProperty, value);
        }

        public Brush FocusColor
        {
            get => (Brush)GetValue(FocusColorProperty);
            set => SetValue(FocusColorProperty, value);
        }

        public double RoundedRadius
        {
            get => (double)GetValue(RoundedRadiusProperty);
            set => SetValue(RoundedRadiusProperty, value);
        }

        public Brush BackgroundColor
        {
            get => (Brush)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }

        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public Brush BorderColor
        {
            get => (Brush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public double BorderSize
        {
            get => (double)GetValue(BorderSizeProperty);
            set => SetValue(BorderSizeProperty, value);
        }

        public Thickness ContentPadding
        {
            get => (Thickness)GetValue(ContentPaddingProperty);
            set => SetValue(ContentPaddingProperty, value);
        }

        public bool ShowErrorMessage
        {
            get => (bool)GetValue(ShowErrorMessageProperty);
            set => SetValue(ShowErrorMessageProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public ITextValidationRule ValidationRule
        {
            get => (ITextValidationRule)GetValue(ValidationRuleProperty);
            set => SetValue(ValidationRuleProperty, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        
        public bool IsError
        {
            get => (bool)GetValue(IsErrorProperty);
            set => SetValue(IsErrorProperty, value);
        }

        public bool IsInputFocus
        {
            get => (bool)GetValue(IsInputFocusProperty);
            set => SetValue(IsInputFocusProperty, value);
        }

        public double InputHeight
        {
            get => (double)GetValue(InputHeightProperty);
            set => SetValue(InputHeightProperty, value);
        }
        
        public bool Readonly
        {
            get => (bool)GetValue(ReadonlyProperty);
            set => SetValue(ReadonlyProperty, value);
        }

        public bool Enable
        {
            get => (bool)GetValue(EnableProperty);
            set => SetValue(EnableProperty, value);
        }

        public int Length
        {
            get => (int)GetValue(LengthProperty);
            set => SetValue(LengthProperty, value);
        }
        
        public abstract System.ComponentModel.DataAnnotations.ValidationResult? Validate();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}