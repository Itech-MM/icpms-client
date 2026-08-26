using System.Globalization;

namespace icpms_client.Utils.Common;

public class CommonUtil
{
    public static string GetWeightDesc(double weight, double? perKgValue = null, string? postfix = "KG")
    {
        if (!perKgValue.HasValue)
        {
            return $"{FormatNumber(weight)} {postfix}";
        }
    
        double finalWeight = weight * perKgValue.Value;
    
        return $"{FormatNumber(finalWeight)} {postfix}";
    }

    public static string FormatNumber(double number)
    {
        return number.ToString("N2", CultureInfo.InvariantCulture);
    }

    
    public static string FormatBytes(long bytes)
    {
        string[] suffixes = { "bytes", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
    
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
    
        return $"{number:n2} {suffixes[counter]}";
    }
}