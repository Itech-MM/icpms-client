namespace icpms_client.Network.DTO.ParkingSlot;

public class ParkingSlotDto: CommonDto
{
	public long SiteId { get; set; }
	public string SiteName { get; set; }
	
    public long ParkingAreaId { get; set; }
    public string ParkingAreaName { get; set; }
	
    public string SlotNumber { get; set; }
    public string FloorLevel { get; set; }
    public bool IsVip  { get; set; } = false;
    public int Status  { get; set; } = 1;
    public string StatusDesc  { get; set; } = "Active";
}