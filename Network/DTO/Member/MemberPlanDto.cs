namespace icpms_client.Network.DTO.Member;

public class MemberPlanDto : CommonDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public decimal? GrantedBalance { get; set; }
    public decimal? DiscountPercent { get; set; }
    public int? FreeMinutes { get; set; }
    public int? DurationDays { get; set; }
    public int? Status { get; set; }
    public string? StatusDesc { get; set; }
}