using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using Newtonsoft.Json;
using icpms_client.Common.UI;
using icpms_client.Network.DTO.Auth;
using icpms_client.Network.DTO.Setting;
using icpms_client.Network.DTO.Shift;
using icpms_client.Network.Request.Auth;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Auth;
using icpms_client.Network.Services.Auth;
using icpms_client.Network.Services.Shift;
using icpms_client.Network.Session;
using icpms_client.Services.UIServices;
using icpms_client.UserControls.Customs.Forms.Validations.Core;
using icpms_client.UserControls.Customs.Forms.Validations.Text;
using icpms_client.UserControls.UI.Reusable.Inputs.Text;
using icpms_client.Utils.Storage;

namespace icpms_client.Pages.Auth;

public sealed partial class LoginPage : Page, INotifyPropertyChanged
{
    private readonly FormValidator _formValidator = new();
    private readonly ILog _log = LogManager.GetLogger(typeof(LoginPage));

    public Action<bool>? OnAuthChanged;

    private readonly AuthService _authService = new();
    private readonly ShiftService _shiftService = new();

    private bool _isLoginEnabled = false;

    private FormTextField[] _pinFields = Array.Empty<FormTextField>();
    private const int PinLength = 6;

    private AuthMethodOption? _selectedAuthMethod;

    public LoginPage()
    {
        InitializeComponent();
        DataContext = this;
        _initializeValidation();
        _initializePinFields();
    }

    public bool IsLoginEnabled
    {
        get => _isLoginEnabled;
        set
        {
            _isLoginEnabled = value;
            OnPropertyChanged(nameof(IsLoginEnabled));
        }
    }

    private void _initializeValidation()
    {
        LoginNameField.ValidationRule = new NotEmptyTextValidationRule("Operator ID");
        PasswordField.ValidationRule = new NotEmptyTextValidationRule("Security Key");

        _formValidator.AddField("Operator ID", LoginNameField);
        _formValidator.AddField("Security Key", PasswordField);
    }

    private void _initializePinFields()
    {
        _pinFields = new[] { Pin1, Pin2, Pin3, Pin4, Pin5, Pin6 };

        for (var i = 0; i < _pinFields.Length; i++)
        {
            var index = i;
            var field = _pinFields[i];

            field.OnTextChanged += value => PinField_OnTextChanged(index, value);
            field.PreviewKeyDown += (_, e) => PinField_OnPreviewKeyDown(index, e);
        }
    }

    private void PinField_OnTextChanged(int index, string? value)
    {
        if (!string.IsNullOrEmpty(value) && index < _pinFields.Length - 1)
        {
            _pinFields[index + 1].FocusInput();
        }

        IsLoginEnabled = _pinFields.All(f => !string.IsNullOrEmpty(f.Value));
    }

    private void PinField_OnPreviewKeyDown(int index, KeyEventArgs e)
    {
        if (e.Key != Key.Back) return;
        if (!string.IsNullOrEmpty(_pinFields[index].Value)) return;

        if (index > 0)
        {
            _pinFields[index - 1].FocusInput();
        }
    }

    private string _getPinValue() => string.Concat(_pinFields.Select(f => f.Value));

    private void _resetPinFields()
    {
        foreach (var f in _pinFields)
        {
            f.Value = string.Empty;
        }
    }

    private async void LoginPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        AuthMethodStatusText.Visibility = Visibility.Visible;
        AuthMethodStatusText.Text = "Loading authentication methods...";

        try
        {
            var response = await _authService.GetAuthMethods();

            if (response is { Success: true })
            {
                var settingsResponse = (BaseResponse<List<SettingDto>>)response;
                var options = AuthMethodMapper.ToOptions(settingsResponse.Data ?? new List<SettingDto>());

                if (options.Count == 0)
                {
                    AuthMethodStatusText.Text = "No authentication methods are configured for this site. Please contact admin.";
                    IsLoginEnabled = false;
                    return;
                }

                AuthMethodChips.ItemsSource = options;
                AuthMethodChips.UpdateLayout();
                SelectAuthMethodChip(options[0]);

                AuthMethodStatusText.Visibility = Visibility.Collapsed;
            }
            else
            {
                AuthMethodStatusText.Text = "Could not load authentication methods. Please contact admin.";
                IsLoginEnabled = false;
            }
        }
        catch (Exception ex)
        {
            _log.Error($"Error loading auth methods: {ex.Message}");
            AuthMethodStatusText.Text = "Could not load authentication methods. Please contact admin.";
            IsLoginEnabled = false;
        }
    }

    private void AuthMethodChip_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: AuthMethodOption option } clicked) return;

        foreach (var toggle in _findVisualChildren<ToggleButton>(AuthMethodChips))
        {
            toggle.IsChecked = ReferenceEquals(toggle, clicked);
        }

        _selectedAuthMethod = option;
        ApplyAuthMethodSelection(option);
    }

    private void SelectAuthMethodChip(AuthMethodOption option)
    {
        foreach (var toggle in _findVisualChildren<ToggleButton>(AuthMethodChips))
        {
            toggle.IsChecked = toggle.Tag is AuthMethodOption o && o.Method == option.Method;
        }

        _selectedAuthMethod = option;
        ApplyAuthMethodSelection(option);
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

    private void ApplyAuthMethodSelection(AuthMethodOption option)
    {
        PasswordAuthPanel.Visibility = Visibility.Collapsed;
        DeviceAuthPanel.Visibility = Visibility.Collapsed;
        PinAuthPanel.Visibility = Visibility.Collapsed;

        if (option.RequiresPin)
        {
            PinAuthPanel.Visibility = Visibility.Visible;
            _resetPinFields();
            IsLoginEnabled = false;
            Dispatcher.BeginInvoke(new Action(() => Pin1.FocusInput()), DispatcherPriority.Background);
        }
        else if (option.RequiresDevice)
        {
            DeviceAuthPanel.Visibility = Visibility.Visible;
            DeviceAuthLabel.Text = $"Present your {option.Label} to the reader";
            IsLoginEnabled = true;
            Dispatcher.BeginInvoke(new Action(() => CredentialField.FocusInput()), DispatcherPriority.Background);
        }
        else
        {
            PasswordAuthPanel.Visibility = Visibility.Visible;
            IsLoginEnabled = true;
            Dispatcher.BeginInvoke(new Action(() => LoginNameField.FocusInput()), DispatcherPriority.Background);
        }
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (_selectedAuthMethod is not { } selected)
        {
            DialogService.ShowErrorDialog("Please select a login method.");
            return;
        }

        try
        {
            if (selected.RequiresPin)
            {
                if (_getPinValue().Length != PinLength)
                {
                    DialogService.ShowErrorDialog("Please enter all 6 digits.");
                    return;
                }
            }
            else if (!selected.RequiresDevice && !_formValidator.Validate())
            {
                return;
            }

            IsLoginEnabled = false;

            var request = new LoginRequest
            {
                AuthMethod = selected.Code
            };

            if (selected.RequiresPin)
            {
                request.Credential = _getPinValue();
            }
            else if (selected.RequiresDevice)
            {
                request.Credential = CredentialField.Value;
            }
            else
            {
                request.Username = LoginNameField.Value;
                request.Password = PasswordField.Value;
            }

            DialogService.ShowLoadingDialog(BaseWindow.MainWindowInstance, "Please wait...");
            var response = await _authService.Login(request);
            DialogService.HideLoading(BaseWindow.MainWindowInstance);

            if (response != null)
            {
                _log.Debug($"Response: {JsonConvert.SerializeObject(response)}");
                if (response.Success)
                {
                    var auth = (BaseResponse<AuthResponse>)response;
                    UserSession.CurrentUser.CurrentAuth = auth.Data;
                    UserSession.CurrentShift = auth.Data!.ActiveShift!;

                    var data = new UserDataObject
                    {
                        CurrentAuth = UserSession.CurrentUser.CurrentAuth
                    };
                    await UserSessionStorage.SaveSessionAsync(data);

                    if (auth.Data is { StartShift: true })
                    {
                        DialogService.ShowStartShiftDialog(async shiftData =>
                        {
                            DialogService.HideStartShiftDialog();
                            if (shiftData != null)
                            {
                                DialogService.ShowLoadingDialog(BaseWindow.MainWindowInstance, "Please wait...");
                                var startShiftResponse = await _shiftService.StartShift(shiftData);
                                DialogService.HideLoading(BaseWindow.MainWindowInstance);

                                if (startShiftResponse is { Success: true })
                                {
                                    var shiftResponse = (BaseResponse<ShiftDto>)startShiftResponse;
                                    if (shiftResponse.Data == null) return;

                                    UserSession.CurrentShift = shiftResponse.Data;
                                    UserSession.CurrentUser.CurrentAuth = auth.Data;
                                    var userData = new UserDataObject
                                    {
                                        CurrentAuth = UserSession.CurrentUser.CurrentAuth
                                    };
                                    await UserSessionStorage.SaveSessionAsync(userData);
                                    OnAuthChanged?.Invoke(true);
                                }
                                else
                                {
                                    DialogService.ShowErrorDialog("Failed to start shift.");
                                }
                            }
                            else
                            {
                                DialogService.ShowErrorDialog("You need to start shift!");
                            }
                        });
                    }
                    else
                    {
                        OnAuthChanged?.Invoke(true);
                    }
                }
                else
                {
                    if (selected.RequiresPin) _resetPinFields();
                    DialogService.ShowErrorDialog(response.Message);
                }
            }
            else
            {
                if (selected.RequiresPin) _resetPinFields();
                DialogService.ShowErrorDialog("Cannot login, please contact admin!");
            }
        }
        catch (Exception ex)
        {
            _log.Error($"Error: {ex.Message}");
        }
        finally
        {
            IsLoginEnabled = true;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}