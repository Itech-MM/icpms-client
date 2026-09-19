using Newtonsoft.Json;

namespace icpms_client.Network.Request.Visitor;

public class VisitorEntryRequest
{
    [JsonProperty("plateNumber")]
    public string PlateNumber { get; set; } = string.Empty;
    
    [JsonProperty("vehicleType")]
    public string VehicleType { get; set; } = string.Empty;
    
    [JsonProperty("parkingSlotId")]
    public long? ParkingSlotId { get; set; }
    
    [JsonProperty("photoUrl")]
    public string? PhotoUrl { get; set; } = string.Empty;
	
    [JsonProperty("platePhotoUrl")]
    public string? PlatePhotoUrl{ get; set; } = string.Empty;
}