namespace icpms_client.Network.DTO.Setting;

public class SettingDto: CommonDto
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Value { get; set; }
    public int? InputType { get; set; }
}