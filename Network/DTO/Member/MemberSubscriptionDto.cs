namespace icpms_client.Network.DTO.Member;

public class MemberSubscriptionDto : CommonDto
{
    public long? MemberId { get; set; }
    public string? MemberName { get; set; }
    public long? PlanId { get; set; }
    public string? PlanName { get; set; }
    public string? StartDateDesc { get; set; }
    public string? EndDateDesc { get; set; }
    public decimal? InitialBalance { get; set; }
    public decimal? Balance { get; set; }
    public int? Status { get; set; }
    public string? StatusDesc { get; set; }
}