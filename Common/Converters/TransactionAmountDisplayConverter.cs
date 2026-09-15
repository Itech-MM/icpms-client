using System.Globalization;
using System.Windows.Data;
using icpms_client.Common.Enums;
using icpms_client.Network.DTO.Member;

namespace icpms_client.Common.Converters;

public class TransactionAmountDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MemberBalanceTransactionDto dto) return string.Empty;

        var amount = dto.Amount ?? 0;

        return (BalanceTransactionType?)dto.TransactionType switch
        {
            BalanceTransactionType.Credit => $"+{amount:N2}",
            BalanceTransactionType.Debit => $"-{amount:N2}",
            BalanceTransactionType.Refund => $"(Refund) {amount:N2}",
            BalanceTransactionType.Adjustment => $"(Adjustment) {amount:N2}",
            _ => amount.ToString("N2")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}