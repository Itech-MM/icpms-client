using System.Windows;
using icpms_client.UserControls.Customs.Forms.Validations.Form;

namespace icpms_client.UserControls.UI.Reusable.Inputs.Text
{
    /// <summary>
    /// Interaction logic for FormPasswordField.xaml
    /// </summary>
    public partial class FormPasswordField : FormField
    {
        private bool _isSyncingFromValue;

        static FormPasswordField()
        {
            ValueProperty.OverrideMetadata(typeof(FormPasswordField),
                new FrameworkPropertyMetadata(default(string),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));
        }

        public FormPasswordField()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler? TextChanged;

        private void txtBox_onChanged(object sender, RoutedEventArgs e)
        {
            if (_isSyncingFromValue) return;

            var password = TxtBox.Password;
            TextChanged?.Invoke(sender, e);
            SetCurrentValue(ValueProperty, password);

            TxtBlock.Visibility = string.IsNullOrEmpty(password) ? Visibility.Visible : Visibility.Hidden;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FormPasswordField field) return;

            var normalized = e.NewValue as string ?? string.Empty;
            if (field.TxtBox.Password == normalized) return;

            field._isSyncingFromValue = true;
            field.TxtBox.Password = normalized;
            field._isSyncingFromValue = false;

            field.TxtBlock.Visibility = string.IsNullOrEmpty(normalized) ? Visibility.Visible : Visibility.Hidden;
        }

        public override System.ComponentModel.DataAnnotations.ValidationResult? Validate()
        {
            var result = ValidationRule.Validate(TxtBox.Password);
            IsError = result != System.ComponentModel.DataAnnotations.ValidationResult.Success;
            if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
            {
                ErrorMessage = result.ErrorMessage ?? "Invalid value.";
                if (ShowErrorMessage)
                {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorMessage = string.Empty;
                ErrorTextBlock.Visibility = Visibility.Collapsed;
            }
            return result;
        }

        private void TxtBlock_OnGotFocus(object sender, RoutedEventArgs e) { }
        private void TxtBlock_OnLostFocus(object sender, RoutedEventArgs e) { }
    }
}