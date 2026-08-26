namespace icpms_client.Utils.General;

public static class StringUtil
{
    public static string FormatNumber(int amount) => amount.ToString("#,##0.00");
    public static string FormatNumber(long amount) => amount.ToString("#,##0.00");
    public static string FormatNumber(double amount) => amount.ToString("#,##0.00");
    public static string FormatNumber(decimal amount) => amount.ToString("#,##0.00");
    public static string FormatNumber(float amount) => amount.ToString("#,##0.00");
}
