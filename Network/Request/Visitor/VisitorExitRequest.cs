using Newtonsoft.Json;

namespace icpms_client.Network.Request.Visitor;

public class VisitorExitRequest
{
    [JsonProperty("plateNumber")]
    public string PlateNumber { get; set; } = string.Empty;
    
    [JsonProperty("paymentMethod")]
    public int PaymentMethod { get; set; }
    
    [JsonProperty("remark")]
    public string? Remark { get; set; }

    [JsonProperty("isMember")]
    public bool IsMember { get; set; }
    
    [JsonProperty("isFoc")]
    public bool IsFoc { get; set; }
    
    
    [JsonProperty("photoUrl")]
    public string? PhotoUrl { get; set; } = string.Empty;
	
    [JsonProperty("platePhotoUrl")]
    public string? PlatePhotoUrl{ get; set; } = string.Empty;
}