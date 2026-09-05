using icpms_client.Network.DTO.ParkingSession;
using icpms_client.Network.DTO.Payment;

namespace icpms_client.Network.Response.Visitor;

public class VisitorExitResponse
{
    public ParkingSessionDto? Session { get; set; }
    public PaymentDto? Payment { get; set; }
    
}