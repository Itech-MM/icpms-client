namespace icpms_client.Network.DTO.Shift;

public class ShiftDto: CommonDto
{
    private string? Code { get; set; }
    private string? StartDateTime { get; set; }
    private string? EndDateTime { get; set; }
    private int? ShiftStatus { get; set; }
    private string? ShiftStatusDesc { get; set; }
}