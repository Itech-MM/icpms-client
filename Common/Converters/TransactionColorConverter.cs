using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using icpms_client.Common.Enums;
using icpms_client.Network.DTO.Member;

namespace icpms_client.Common.Converters;

public class TransactionColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MemberBalanceTransactionDto dto) return Application.Current.FindResource("PrimaryBlackColor");

        var resourceKey = (BalanceTransactionType?)dto.TransactionType switch
        {
            BalanceTransactionType.Credit => "PrimaryGreenColor",
            BalanceTransactionType.Debit => "PrimaryRedColor",
            BalanceTransactionType.Refund => "InfoChipTextColor",
            BalanceTransactionType.Adjustment => "WarningChipTextColor",
            _ => "PrimaryBlackColor"
        };

        return Application.Current.FindResource(resourceKey);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}