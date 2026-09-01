namespace icpms_client.Network.DTO.Vehicle;

public class VehicleDto:CommonDto
{
    public string PlateNumber { get; set; } = string.Empty;
    	public string VehicleType { get; set; } = string.Empty;
    	public long MemberId { get; set; } = -1;
    	public string MemberName { get; set; } = string.Empty;
    	public int Status { get; set; } = 1;
    	public string StatusDesc { get; set; } = string.Empty;
}