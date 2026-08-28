using icpms_client.Network.DTO.Gate;

namespace icpms_client.Network.DTO.Shift;

public class ShiftDto: CommonDto
{
    public string Code { get; set; }
    public long OperatorId { get; set; }
    public GateDto Gate { get; set; }
    public string? StartDateTime { get; set; }
    public string? EndDateTime { get; set; }
    public int? ShiftStatus { get; set; }
    public string? ShiftStatusDesc { get; set; }
    public decimal? OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? Diff { get; set; }
    
    public string? OpeningCashDesc { get; set; }
    public string? ClosingCashDesc { get; set; }
    public string? DiffDesc { get; set; }
    
    public string? Remark { get; set; }
}