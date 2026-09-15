using Newtonsoft.Json;

namespace icpms_client.Network.DTO.Member;

public class MemberDto : CommonDto
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("membershipType")]
    public int? MembershipType { get; set; }

    [JsonProperty("membershipTypeDesc")]
    public string MembershipTypeDesc { get; set; } = string.Empty;

    [JsonProperty("isVip")]
    public bool? IsVip { get; set; } = false;

    [JsonProperty("validUntil")]
    public string ValidUntil { get; set; } = string.Empty;

    [JsonProperty("reservedSlotId")]
    public long? ReservedSlotId { get; set; }

    [JsonProperty("reservedSlotNumber")]
    public string ReservedSlotNumber { get; set; } = string.Empty;

    [JsonProperty("status")]
    public int? Status { get; set; } = 1;

    [JsonProperty("statusDesc")]
    public string? StatusDesc { get; set; }

    [JsonProperty("isExpired")]
    public bool? IsExpired { get; set; } = false;
}