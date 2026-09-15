using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using icpms_client.UserControls.UI.Reusable.Inputs.Formatter.Helpers;
using icpms_client.UserControls.UI.Reusable.Utils;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace icpms_client.UserControls.UI.Reusable.Inputs.Text
{
    public partial class FormTextArea
    {
        public FormTextArea()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                IsError = false;
                IsInputFocus = false;
                BorderColor = Brushes.Black;
                FocusColor = Brushes.Blue;
                return;
            }
            InitializeComponent();
        }

        public event Action<string?>? OnTextChanged;

        public bool TextChanged;

        private void txtBox_onChanged(object sender, TextChangedEventArgs e)
        {
            TxtBlock.Visibility =
                string.IsNullOrEmpty(TxtBox.Text) ? Visibility.Visible : Visibility.Hidden; 
                
            if (!Readonly)
            {
                SetCurrentValue(ValueProperty, TxtBox.Text);
            }

            OnTextChanged?.Invoke(TxtBox.Text);
            TextChanged = true;
        }

        public override ValidationResult Validate()
        {
            var validationResult = ValidationRule?.Validate(TxtBox.Text) ?? ValidationResult.Success;
            IsError = validationResult != ValidationResult.Success;
            
            if (validationResult != ValidationResult.Success)
            {
                ErrorMessage = validationResult.ErrorMessage ?? "Invalid value.";
            }
            else
            {
                ErrorMessage = string.Empty;
            }
            
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                if (ShowErrorMessage)
                {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorTextBlock.Visibility = Visibility.Collapsed;
            }

            return validationResult!;
        }

        private void TxtBlock_OnGotFocus(object sender, RoutedEventArgs e)
        {
            IsInputFocus = true;
        }

        private void TxtBlock_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            OnTextChanged?.Invoke(textBox?.Text);
            IsInputFocus = false;
        }

        private void TxtBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var newText = TxtBox.Text.Insert(TxtBox.CaretIndex, e.Text);
            if (Length != 0 && newText.Length > Length)
            {
                e.Handled = true;
                return;
            }

            var allow = FormattingHelper.IsAllowText(TextFormatting, newText);
            e.Handled = !allow;
        }
    }
}