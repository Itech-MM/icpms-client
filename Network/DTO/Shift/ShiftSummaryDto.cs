namespace icpms_client.Network.DTO.Shift;

public class ShiftSummaryDto
{
    public string Code { get; set; } = string.Empty;
    public int TotalTransactions  { get; set; } = 0;
    public decimal TotalAmount  { get; set; } = 0;
    public string TotalAmountDesc { get; set; } = "0";
    public int TotalIncompleteTransactions { get; set; } = 0;
    public int TotalCompletedTransactions  { get; set; } = 0;
}