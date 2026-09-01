using Newtonsoft.Json;

namespace icpms_client.Network.Request.Visitor;

public class VisitorEntryRequest
{
    [JsonProperty("plateNumber")]
    public string PlateNumber { get; set; } = String.Empty;
    
    [JsonProperty("vehicleType")]
    public string VehicleType { get; set; } = String.Empty;
    
    [JsonProperty("parkingSlotId")]
    public long? ParkingSlotId { get; set; }
}