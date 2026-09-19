using System.Globalization;
using System.Text.RegularExpressions;
using icpms_client.Network.DTO.Setting;
using icpms_client.Utils.General;

namespace icpms_client.Common.ContextData;

public static class CommonData
{
    public static string FtpFolderPath { get; set; } = string.Empty;
    public static bool IsInitialize { get; set; } = false;

    public static CommonApplicationSettings ApplicationSettings { get; set; } = new();

    public static void InitializeSettings(List<SettingDto> settings)
    {
        if (IsInitialize) return;

        var appSettings = new CommonApplicationSettings();

        foreach (var setting in settings)
        {
            if (string.IsNullOrWhiteSpace(setting.Code)) continue;

            try
            {
                switch (setting.Code)
                {
                    case SettingCodes.AllowUnknownNumber:
                        appSettings.AllowUnknownNumber = ParseToggle(setting.InputType, setting.Value, setting.Code);
                        break;
                    case SettingCodes.FtpHost:
                        appSettings.FtpPath = NormalizeValueByInputType(setting.InputType, setting.Value, setting.Code);
                        break;
                    case SettingCodes.FtpUser:
                        appSettings.FtpUser = NormalizeValueByInputType(setting.InputType, setting.Value, setting.Code);
                        break;
                    case SettingCodes.FtpPassword:
                        appSettings.FtpPassword = NormalizeValueByInputType(setting.InputType, setting.Value, setting.Code);
                        break;
                    case SettingCodes.FtpFolderPath:
                        appSettings.FtpFolderPath = NormalizeValueByInputType(setting.InputType, setting.Value, setting.Code);
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                // A malformed setting shouldn't crash app startup — log and skip it, leave the field unset
                Console.WriteLine($"Skipping invalid setting '{setting.Code}': {ex.Message}");
            }
        }

        ApplicationSettings = appSettings;
        FtpFolderPath = appSettings.FtpFolderPath ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(appSettings.FtpPath) &&
            !string.IsNullOrWhiteSpace(appSettings.FtpUser))
        {
            InitializeFtp(appSettings.FtpPath, appSettings.FtpUser, appSettings.FtpPassword ?? string.Empty);
        }

        IsInitialize = true;
    }

    private static string? NormalizeValueByInputType(int? inputTypeCode, string? rawValue, string? settingCode)
    {
        if (inputTypeCode == null)
        {
            return rawValue?.Trim();
        }

        if (!Enum.IsDefined(typeof(SettingInputType), inputTypeCode.Value))
        {
            return rawValue?.Trim();
        }

        var type = (SettingInputType)inputTypeCode.Value;

        switch (type)
        {
            case SettingInputType.Number:
            {
                var trimmed = rawValue?.Trim();
                if (string.IsNullOrEmpty(trimmed) || !Regex.IsMatch(trimmed, @"^-?\d+(\.\d+)?$"))
                {
                    throw new ArgumentException($"Invalid number value for setting: {settingCode}");
                }
                return trimmed;
            }

            case SettingInputType.Toggle:
            {
                var v = rawValue?.Trim().ToLowerInvariant() ?? string.Empty;
                var isTrue = v is "true" or "1" or "on" or "yes";
                return isTrue ? "1" : "0";
            }

            case SettingInputType.Date:
            {
                var trimmed = rawValue?.Trim();
                if (string.IsNullOrEmpty(trimmed) ||
                    !DateTime.TryParseExact(trimmed, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out _))
                {
                    throw new ArgumentException($"Invalid date value for setting: {settingCode}");
                }
                return trimmed;
            }

            case SettingInputType.Password:
                return rawValue?.Trim();

            case SettingInputType.Text:
            default:
                return rawValue?.Trim();
        }
    }

// Toggle-specific helper for callers that need a bool, not the normalized "1"/"0" string
    private static bool ParseToggle(int? inputTypeCode, string? rawValue, string? settingCode)
    {
        var normalized = NormalizeValueByInputType(inputTypeCode, rawValue, settingCode);
        return normalized == "1";
    }

    public static void InitializeFtp(string host, string userName, string password)
    {
        CommonData.FtpUtil?.Dispose();
        FtpUtil = new FtpUtil(host, userName, password);
    }

    public static FtpUtil? FtpUtil { get; private set; }
}

public static class SettingCodes
{
    public const string AllowUnknownNumber = "ALLOW_UNKNOWN_NUMBER";
    public const string FtpHost = "FTP_PATH";
    public const string FtpUser = "FTP_USER";
    public const string FtpPassword = "FTP_PASSWORD";
    public const string FtpFolderPath = "FTP_IMAGE_PATH";
}
public enum SettingInputType
{
    Text = 1,
    Number = 2,
    Toggle = 3,
    Date = 4,
    Password = 5
}

public class CommonApplicationSettings
{
    public bool? AllowUnknownNumber { get; set; }
    public string? FtpPath { get; set; }
    public string? FtpUser { get; set; }
    public string? FtpPassword { get; set; }
    public string? FtpFolderPath { get; set; }
}