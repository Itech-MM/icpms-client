using System.Text;
using System.Text.RegularExpressions;

namespace icpms_client.Utils.Converters;

public class WeightIndicatorDataConverter
{
    public static double Convert(string rawMessage)
    {
        try
        {
            if (rawMessage.Length > 9)
            {
                var removeTailing = rawMessage.Substring(0, rawMessage.Length - 4);
                var finalMessage = removeTailing.Substring(1);
                return double.Parse(finalMessage);
            }
            return 0.0;
        }
        catch
        {
            return 0.0;
        }
    }
}