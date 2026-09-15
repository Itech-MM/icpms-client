namespace icpms_client.Network.DTO.Member;

public class MemberBalanceTransactionDto : CommonDto
{
    public long? SubscriptionId { get; set; }
    public long? MemberId { get; set; }
    public long? SessionId { get; set; }
    public int? TransactionType { get; set; }
    public string? TransactionTypeDesc { get; set; }
    public decimal? Amount { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string? Remark { get; set; }
}