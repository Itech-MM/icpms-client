using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using icpms_client.ViewModels.Wrappers;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace icpms_client.UserControls.UI.Reusable.Inputs.Text;

public partial class FormSearchTextField
{
    
    public static readonly DependencyProperty IsPopupOpenProperty =
        DependencyProperty.Register(nameof(IsPopupOpen), typeof(bool), typeof(FormSearchTextField), new PropertyMetadata(false));
    
    public static readonly DependencyProperty SelectedProperty =
        DependencyProperty.Register(nameof(Selected), typeof(LabelValueWrapper), typeof(FormSearchTextField), new PropertyMetadata(LabelValueWrapper.Empty));
    
    public static readonly DependencyProperty IconProperty = 
        DependencyProperty.Register(nameof(Icon), typeof(FontAwesomeIcon), typeof(FormSearchTextField), new PropertyMetadata(FontAwesomeIcon.Search));
    
    public static readonly DependencyProperty NotEmptyProperty = 
        DependencyProperty.Register(nameof(NotEmpty), typeof(bool), typeof(FormSearchTextField), new PropertyMetadata(false));

    
    public ObservableCollection<LabelValueWrapper> SearchResults { get; set; } = [];
    
    public event Action<LabelValueWrapper>? OnValueChanged;
    
    public event Action<string?>? OnTextChanged; 

    public event Action<string>? OnSearch;

    private bool _styleOpen;
    
    private readonly DispatcherTimer _debounceTimer;
    
    private string _recentKeyword = string.Empty;
    
    public FormSearchTextField()
    {
        InitializeComponent();
        SearchField.ValidationRule = ValidationRule;
        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _debounceTimer.Tick += DebounceTimer_Tick;
    }

    public bool StayOpen
    {
        get => _styleOpen;
        set
        {
            _styleOpen = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsPopupOpen
    {
        get => (bool)GetValue(IsPopupOpenProperty);
        set
        {
            SetValue(IsPopupOpenProperty, value);
            OnPropertyChanged();
        }
    }
    
    public bool NotEmpty
    {
        get => (bool)GetValue(NotEmptyProperty);
        set
        {
            SetValue(NotEmptyProperty, value);
            OnPropertyChanged();
        }
    }

    public LabelValueWrapper Selected
    {
        get=> (LabelValueWrapper)GetValue(SelectedProperty);
        set
        {
            SetValue(SelectedProperty, value);
            /*SearchResults.Add(value);*/
            OnPropertyChanged();
        }
    }

    public FontAwesomeIcon Icon
    {
        get => (FontAwesomeIcon)GetValue(IconProperty);
        set
        {
            SetValue(IconProperty, value);
            OnPropertyChanged();
        }
    }
    
    private void OnSearch_Click(object sender, RoutedEventArgs e)
    {
        var value = SearchField.Value;
        
        OnSearch?.Invoke(value);
    }
    
    private void OnResult_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { CommandParameter: LabelValueWrapper labelValueWrapper })
        {
            Selected = labelValueWrapper;
            SearchField.Value = Selected.Label;
            OnValueChanged?.Invoke(labelValueWrapper);
        }
        IsPopupOpen = false;
    }
    
    public override ValidationResult Validate()
    {
        var validationResult = ValidationRule.Validate(SearchField.Value);
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


    private void TextField_OnOnTextChanged(string? obj)
    {
       OnTextChanged?.Invoke(obj);
    }

    private void SearchField_OnKeyDown(object sender, KeyEventArgs e)
    {
        
        
        if (SearchField.Value == _recentKeyword) return;
        
        _debounceTimer.Stop();
        _debounceTimer.Start();

        _recentKeyword = SearchField.Value;
        
    }
    
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        Unloaded += (_, _) => _debounceTimer.Stop();
    }

    private void SearchField_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Keyboard.Focus(SearchField);
        var value = SearchField.Value;
        if (value.Trim() == string.Empty && NotEmpty) return;
        OnSearch?.Invoke(value);
        StayOpen = true;
    }

    private void SearchField_OnLostFocus(object sender, RoutedEventArgs e)
    {
        StayOpen = false;
        _debounceTimer.Stop();
    }
    
    private void DebounceTimer_Tick(object? sender, EventArgs e)
    {
        _debounceTimer.Stop();
    
        var value = SearchField.Value;
        if (value.Trim() == string.Empty && NotEmpty) return;
        OnSearch?.Invoke(value);
    }

    private void SearchField_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var effectiveKey = e.Key == Key.System ? e.SystemKey : e.Key;
        Console.WriteLine($"PreviewKeyDown: {effectiveKey}");

        if (effectiveKey == Key.Down)
        {
            HandleDownArrow();
            e.Handled = true; // Block event from propagating
        }

        if (effectiveKey == Key.Back)
        {
            if (SearchField.Value == _recentKeyword) return;
            _debounceTimer.Stop();
            _debounceTimer.Start();
            _recentKeyword = SearchField.Value;
            
        }
    }
    
    private void HandleDownArrow()
    {
        if (ItemsControl.Items.Count == 0)
        {
            Debug.WriteLine("No items to navigate");
            return;
        }

        if (ItemsControl.ItemContainerGenerator
                .ContainerFromIndex(0) is ContentPresenter container)
        {
            // Find the Button inside the ContentPresenter
            var button = FindVisualChild<Button>(container);

            Debug.WriteLine("Focusing first item");
            button?.Focus();
            IsPopupOpen = true;
        }
    }
    
    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent);)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            var descendant = FindVisualChild<T>(child);
            return descendant;
        }
        return null;
    }
}