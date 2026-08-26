using System.Windows;
using System.Windows.Media;
using icpms_client.UserControls.Customs.Forms.Validations.Form;
using icpms_client.UserControls.UI.Reusable.Utils;

namespace icpms_client.UserControls.UI.Reusable.Inputs.Text
{
    /// <summary>
    /// Interaction logic for FormPasswordField.xaml
    /// </summary>
    public partial class FormPasswordField : FormField
    {
        public FormPasswordField()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public event RoutedEventHandler? TextChanged;

        private void txtBox_onChanged(object sender, RoutedEventArgs e)
        {
            var password = TxtBox.Password;  // Access the password entered in PasswordBox
            // Invoke the TextChanged event, if any subscribers exist
            TextChanged?.Invoke(sender, e);
            Value = password;

            TxtBlock.Visibility = string.IsNullOrEmpty(password) ? Visibility.Visible : Visibility.Hidden;
        }

        public override System.ComponentModel.DataAnnotations.ValidationResult? Validate()
        {
            {
                var result = ValidationRule.Validate(TxtBox.Password);
                IsError = result != System.ComponentModel.DataAnnotations.ValidationResult.Success;
                if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                {
                    // Show error message if validation fails
                    ErrorMessage = result.ErrorMessage ?? "Invalid value.";
                    if (ShowErrorMessage)
                    {
                        ErrorTextBlock.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    // Clear error message if validation succeeds
                    ErrorMessage = string.Empty;
                    ErrorTextBlock.Visibility = Visibility.Collapsed;
                }
                return result;
            }
        }
        
        private void TxtBlock_OnGotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void TxtBlock_OnLostFocus(object sender, RoutedEventArgs e)
        {

        }

    }
}