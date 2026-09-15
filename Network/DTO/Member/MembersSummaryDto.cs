namespace icpms_client.Network.DTO.Member;

public class MembersSummaryDto
{
    public long TotalMembers { get; set; }
    public long RegularMembers { get; set; }
    public long VipMembers { get; set; }
    public long ExpiredMembers { get; set; }
    public long ActiveMembers { get; set; }
}