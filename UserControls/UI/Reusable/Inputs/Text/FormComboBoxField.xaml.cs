using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using icpms_client.UserControls.Customs.Forms.Validations.Form;
using icpms_client.UserControls.UI.Reusable.Utils;
using icpms_client.ViewModels.Wrappers;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace icpms_client.UserControls.UI.Reusable.Inputs.Text;

public partial class FormComboBoxField 
{
    public static readonly DependencyProperty IsPopupOpenProperty =
        DependencyProperty.Register(nameof(IsPopupOpen), typeof(bool), typeof(FormComboBoxField), new PropertyMetadata(false));

    public static readonly DependencyProperty SelectedProperty =
        DependencyProperty.Register(nameof(Selected), typeof(LabelValueWrapper), typeof(FormComboBoxField), new PropertyMetadata(null));

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<LabelValueWrapper>), typeof(FormComboBoxField), new PropertyMetadata(null, OnItemsChanged));

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FormComboBoxField)d;
        control.OnPropertyChanged(nameof(Items));
    }
    public ObservableCollection<LabelValueWrapper> Items
    {
        get=> (ObservableCollection<LabelValueWrapper>)GetValue(ItemsProperty);
        set
        {
            SetValue(ItemsProperty, value);
            OnPropertyChanged(nameof(Items));
        }
    }

    public event Action<LabelValueWrapper>? OnValueChanged;

    public event Action<LabelValueWrapper?>? OnSearch; 
    public FormComboBoxField()
    {
        InitializeComponent();
        TextField.ValidationRule = ValidationRule;
    }

    public LabelValueWrapper Selected
    {
        get=> (LabelValueWrapper)GetValue(SelectedProperty);
        set
        {
            SetValue(SelectedProperty, value);
            /*Items.Add(value);*/
            OnPropertyChanged(nameof(Selected));
        }
    }
    
    public bool IsPopupOpen
    {
        get => (bool)GetValue(IsPopupOpenProperty);
        set => SetValue(IsPopupOpenProperty, value);
    }
    
    public override ValidationResult Validate()
    {
        
        var validationResult = ValidationRule.Validate(TextField.Value);
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
            // Clear error message and reset border color for valid input
            ErrorTextBlock.Visibility = Visibility.Collapsed;

        }
        return validationResult;
    }

    private void OnSearch_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
            IsPopupOpen = true;
            OnSearch?.Invoke(Selected);
            ItemsControl.Focus();
    }

    private void OnResult_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { CommandParameter: LabelValueWrapper labelValueWrapper })
        {
            Selected = labelValueWrapper;
            OnValueChanged?.Invoke(labelValueWrapper);
        }
        
        IsPopupOpen = false;
    }

    private void OnGotFocus(object sender, EventArgs e)
    {
    }

    private void OnLostFocus(object sender, EventArgs e)
    {
        
    }
}