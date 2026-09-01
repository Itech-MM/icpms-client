namespace icpms_client.Network.DTO.Member;

public class MemberDto : CommonDto
{
    public string Name { get; set; } =  string.Empty;
    public string PhoneNumber { get; set;}  =  string.Empty;
    public string Email { get; set; } =  string.Empty;
    public int? MembershipType { get; set; }
    public string MembershipTypeDesc { get; set; } =  string.Empty;
    public bool? IsVip { get; set; } = false;
    public string ValidUntil { get; set; } =  string.Empty;
    public long? ReservedSlotId { get; set; }
    public string ReservedSlotNumber { get; set; } =  string.Empty;
    public int? Status { get; set; } = 1;
    public string? StatusDesc { get; set; }
    public bool? IsExpired { get; set; } = false;
}