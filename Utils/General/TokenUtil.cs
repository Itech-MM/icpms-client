using System.Security.Cryptography;

namespace icpms_client.Utils.General;

public class TokenUtil
{
    public static string GenerateTransactionCode()
    {
        byte[] buffer = new byte[40];
        RandomNumberGenerator.Fill(buffer); // Modern secure random number generation
        var bigInteger = new System.Numerics.BigInteger(buffer);
        var code = bigInteger.ToString("X").Substring(0, 6).ToUpper();
        return "TX" + code;
    }
}