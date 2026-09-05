using System.ComponentModel.DataAnnotations;

namespace icpms_client.Network.DTO.Payment;

public class PaymentDto : CommonDto
{
    [Required]
    public long SessionId { get; set; }

    public string? PlateNumber { get; set; }

    public string? SiteName { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public string? AmountDesc { get; set; }

    [Required]
    public int Method { get; set; }

    public string? MethodDesc { get; set; }

    public string? PaymentTime { get; set; }

    public string? ReferenceNo { get; set; }

    public int Status { get; set; } = 2;

    public string? StatusDesc { get; set; }

    public PaymentDto()
    {
    }

    public PaymentDto(
        long sessionId,
        string? plateNumber,
        string? siteName,
        decimal amount,
        string? amountDesc,
        int method,
        string? methodDesc,
        string? paymentTime,
        string? referenceNo,
        int status,
        string? statusDesc)
    {
        SessionId = sessionId;
        PlateNumber = plateNumber;
        SiteName = siteName;
        Amount = amount;
        AmountDesc = amountDesc;
        Method = method;
        MethodDesc = methodDesc;
        PaymentTime = paymentTime;
        ReferenceNo = referenceNo;
        Status = status;
        StatusDesc = statusDesc;
    }
}