using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Newtonsoft.Json;
using icpms_client.Common.UI;
using icpms_client.Network.Request.Auth;
using icpms_client.Network.Response;
using icpms_client.Network.Response.Auth;
using icpms_client.Network.Services.Auth;
using icpms_client.Network.Session;
using icpms_client.Services.UIServices;
using icpms_client.UserControls.Customs.Forms.Validations.Core;
using icpms_client.UserControls.Customs.Forms.Validations.Text;
using icpms_client.Utils.Storage;

namespace icpms_client.Pages.Auth;

public sealed partial class LoginPage : Page, INotifyPropertyChanged
{
    private readonly FormValidator _formValidator = new FormValidator();
    private readonly ILog _log = LogManager.GetLogger(typeof(LoginPage));

    public Action<bool>? OnAuthChanged;

    private readonly AuthService _authService = new();

    private bool _isLoginEnabled = true;

    public LoginPage()
    {
        InitializeComponent();
        DataContext = this;
        _initializeValidation();
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

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_formValidator.Validate()) return;
            IsLoginEnabled = false;

            var request = new LoginRequest
            {
                Username = LoginNameField.Value,
                Password = PasswordField.Value
            };

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

                    var data = new UserDataObject
                    {
                        CurrentAuth = UserSession.CurrentUser.CurrentAuth,
                    };
                    await UserSessionStorage.SaveSessionAsync(data);
                    OnAuthChanged?.Invoke(true);
                }
                else
                {
                    DialogService.ShowErrorDialog(response.Message);
                }
            }
            else
            {
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