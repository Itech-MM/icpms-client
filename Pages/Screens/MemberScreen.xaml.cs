using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using icpms_client.Network.DTO.Auth;
using icpms_client.Network.DTO.Setting;
using icpms_client.Network.Request.Member;
using icpms_client.Network.Response;
using icpms_client.Network.Services.Auth;
using icpms_client.Pages.Screens.ViewModels;
using icpms_client.Services.UIServices;
using icpms_client.UserControls.UI.Reusable.Inputs.Text;
using Microsoft.Extensions.DependencyInjection;

namespace icpms_client.Pages.Screens;

public partial class MemberScreen
{
    private readonly MemberScreenViewModel _viewModel;
    private readonly AuthService _authService = new();

    private FormTextField[] _supervisorPinFields = Array.Empty<FormTextField>();
    private const int SupervisorPinLength = 6;

    private AuthMethodOption? _selectedSupervisorAuthMethod;
    private bool _supervisorMethodsLoaded;

    public MemberScreen()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider!.GetRequiredService<MemberScreenViewModel>();
        DataContext = _viewModel;

        Loaded += MemberScreen_Loaded;
        _initializeSupervisorPinFields();
        _viewModel.PropertyChanged += ViewModel_OnPropertyChanged;
    }

    private async void MemberScreen_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private void ViewModel_OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MemberScreenViewModel.IsSupervisorModalOpen)) return;
        if (_viewModel.IsSupervisorModalOpen)
        {
            _resetSupervisorModal();
        }
    }

    private void _initializeSupervisorPinFields()
    {
        _supervisorPinFields = [SupPin1, SupPin2, SupPin3, SupPin4, SupPin5, SupPin6];

        for (var i = 0; i < _supervisorPinFields.Length; i++)
        {
            var index = i;
            var field = _supervisorPinFields[i];

            field.OnTextChanged += value => _supervisorPinField_OnTextChanged(index, value);
            field.PreviewKeyDown += (_, e) => _supervisorPinField_OnPreviewKeyDown(index, e);
        }
    }

    private void _supervisorPinField_OnTextChanged(int index, string? value)
    {
        if (!string.IsNullOrEmpty(value) && index < _supervisorPinFields.Length - 1)
        {
            _supervisorPinFields[index + 1].FocusInput();
        }
    }

    private void _supervisorPinField_OnPreviewKeyDown(int index, KeyEventArgs e)
    {
        if (e.Key != Key.Back) return;
        if (!string.IsNullOrEmpty(_supervisorPinFields[index].Value)) return;

        if (index > 0)
        {
            _supervisorPinFields[index - 1].FocusInput();
        }
    }

    private string _getSupervisorPinValue() => string.Concat(_supervisorPinFields.Select(f => f.Value));

    private void _resetSupervisorPinFields()
    {
        foreach (var f in _supervisorPinFields) f.Value = string.Empty;
    }

    private async void _resetSupervisorModal()
    {
        SupervisorUsernameField.Value = string.Empty;
        SupervisorPasswordField.Value = string.Empty;
        SupervisorCredentialField.Value = string.Empty;
        _resetSupervisorPinFields();

        if (!_supervisorMethodsLoaded)
        {
            try
            {
                var response = await _authService.GetAuthMethods();
                if (response is { Success: true })
                {
                    var settingsResponse = (BaseResponse<List<SettingDto>>)response;
                    var options = AuthMethodMapper.ToOptions(settingsResponse.Data ?? new List<SettingDto>());
                    SupervisorAuthMethodChips.ItemsSource = options;
                    SupervisorAuthMethodChips.UpdateLayout();
                    _supervisorMethodsLoaded = true;

                    if (options.Count > 0)
                    {
                        _selectSupervisorAuthMethodChip(options[0]);
                    }
                }
            }
            catch
            {
                // supervisor modal stays with no method selected; Confirm click validates this
            }
        }
        else if (_selectedSupervisorAuthMethod != null)
        {
            _selectSupervisorAuthMethodChip(_selectedSupervisorAuthMethod);
        }
    }

    private void SupervisorAuthMethodChip_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: AuthMethodOption option } clicked) return;

        foreach (var toggle in _findVisualChildren<ToggleButton>(SupervisorAuthMethodChips))
        {
            toggle.IsChecked = ReferenceEquals(toggle, clicked);
        }

        _selectedSupervisorAuthMethod = option;
        _applySupervisorAuthMethodSelection(option);
    }

    private void _selectSupervisorAuthMethodChip(AuthMethodOption option)
    {
        foreach (var toggle in _findVisualChildren<ToggleButton>(SupervisorAuthMethodChips))
        {
            toggle.IsChecked = toggle.Tag is AuthMethodOption o && o.Method == option.Method;
        }

        _selectedSupervisorAuthMethod = option;
        _applySupervisorAuthMethodSelection(option);
    }

    private void _applySupervisorAuthMethodSelection(AuthMethodOption option)
    {
        SupervisorPasswordAuthPanel.Visibility = Visibility.Collapsed;
        SupervisorDeviceAuthPanel.Visibility = Visibility.Collapsed;
        SupervisorPinAuthPanel.Visibility = Visibility.Collapsed;

        if (option.RequiresPin)
        {
            SupervisorPinAuthPanel.Visibility = Visibility.Visible;
            _resetSupervisorPinFields();
            Dispatcher.BeginInvoke(new Action(() => SupPin1.FocusInput()), DispatcherPriority.Background);
        }
        else if (option.RequiresDevice)
        {
            SupervisorDeviceAuthPanel.Visibility = Visibility.Visible;
            SupervisorDeviceAuthLabel.Text = $"Present supervisor's {option.Label} to the reader";
            Dispatcher.BeginInvoke(new Action(() => SupervisorCredentialField.FocusInput()), DispatcherPriority.Background);
        }
        else
        {
            SupervisorPasswordAuthPanel.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(new Action(() => SupervisorUsernameField.FocusInput()), DispatcherPriority.Background);
        }
    }

    private static IEnumerable<T> _findVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) yield return typed;
            foreach (var grandChild in _findVisualChildren<T>(child)) yield return grandChild;
        }
    }

    private async void ConfirmSupervisorButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedSupervisorAuthMethod is not { } selected)
        {
            DialogService.ShowErrorDialog("Please select a verification method.");
            return;
        }

        var request = new SupervisorApprovalRequest { AuthMethod = selected.Code };

        if (selected.RequiresPin)
        {
            if (_getSupervisorPinValue().Length != SupervisorPinLength)
            {
                DialogService.ShowErrorDialog("Please enter all 6 digits.");
                return;
            }
            request.Credential = _getSupervisorPinValue();
        }
        else if (selected.RequiresDevice)
        {
            if (string.IsNullOrWhiteSpace(SupervisorCredentialField.Value))
            {
                DialogService.ShowErrorDialog("Please present the supervisor's credential.");
                return;
            }
            request.Credential = SupervisorCredentialField.Value;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(SupervisorUsernameField.Value) || string.IsNullOrWhiteSpace(SupervisorPasswordField.Value))
            {
                DialogService.ShowErrorDialog("Please enter supervisor username and password.");
                return;
            }
            request.Username = SupervisorUsernameField.Value;
            request.Password = SupervisorPasswordField.Value;
        }

        var success = await _viewModel.ConfirmSupervisorApprovalAsync(request);
        if (!success && selected.RequiresPin)
        {
            _resetSupervisorPinFields();
        }
    }
}