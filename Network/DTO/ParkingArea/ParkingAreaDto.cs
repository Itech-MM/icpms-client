namespace icpms_client.Network.DTO.ParkingArea;

public class ParkingAreaDto: CommonDto
{
    public string Name { get; set; } = string.Empty;

    public string? Remark { get; set; }

    public int TotalSlot { get; set; } = 0;

    public int VipSlot { get; set; } = 0;

    public int NormalSlot { get; set; } = 0;

    public int AvailableTotalSlot { get; set; } = 0;

    public int AvailableTotalVipSlot { get; set; } = 0;

    public int AvailableTotalNormalSlot { get; set; } = 0;

    public bool SlotTrackingEnabled;

    public int Status { get; set; } = 1;
    public string StatusDesc;
	

    public long TariffId { get; set; } = -1;
    public string TariffName { get; set; }
}