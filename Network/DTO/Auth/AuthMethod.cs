using FontAwesome.WPF;
using icpms_client.Network.DTO.Setting;

namespace icpms_client.Network.DTO.Auth;

public enum AuthMethod
{
    Password = 1,
    Rfid = 2,
    QrCode = 3,
    MagStripe = 4,
    Pin = 5
}

public class AuthMethodOption
{
    public AuthMethod Method { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool RequiresDevice { get; set; }
    public bool RequiresPin => Method == AuthMethod.Pin;
    public FontAwesomeIcon Icon { get; set; }
    public int Code => (int)Method;
}

public static class AuthMethodMapper
{
    private static readonly Dictionary<string, AuthMethod> CodeMap = new()
    {
        { "AUTH_METHOD_PASSWORD", AuthMethod.Password },
        { "AUTH_METHOD_RFID", AuthMethod.Rfid },
        { "AUTH_METHOD_QR", AuthMethod.QrCode },
        { "AUTH_METHOD_SWIPE", AuthMethod.MagStripe },
        { "AUTH_METHOD_PIN", AuthMethod.Pin }
    };

    private static readonly Dictionary<AuthMethod, (string Label, bool RequiresDevice, FontAwesomeIcon Icon)> InfoMap = new()
    {
        { AuthMethod.Password, ("Username/Password", false, FontAwesomeIcon.Key) },
        { AuthMethod.Rfid, ("RFID Card", true, FontAwesomeIcon.IdCard) },
        { AuthMethod.QrCode, ("QR Code Scan", true, FontAwesomeIcon.Qrcode) },
        { AuthMethod.MagStripe, ("Magnetic Stripe Swipe", true, FontAwesomeIcon.CreditCard) },
        { AuthMethod.Pin, ("Quick PIN", false, FontAwesomeIcon.Hashtag) }
    };

    public static List<AuthMethodOption> ToOptions(IEnumerable<SettingDto> settings)
    {
        var options = new List<AuthMethodOption>();
        foreach (var setting in settings)
        {
            if (setting.Code == null) continue;
            if (!IsToggleOn(setting.Value)) continue;
            if (!CodeMap.TryGetValue(setting.Code, out var method)) continue;

            var info = InfoMap[method];
            options.Add(new AuthMethodOption
            {
                Method = method,
                Label = info.Label,
                RequiresDevice = info.RequiresDevice,
                Icon = info.Icon
            });
        }
        return options;
    }

    private static bool IsToggleOn(string? value)
    {
        var v = value?.Trim().ToLowerInvariant() ?? string.Empty;
        return v is "true" or "1" or "on" or "yes";
    }
}