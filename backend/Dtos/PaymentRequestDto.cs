namespace BE_DA_WEB.Dtos;
public class PaymentRequestDto
{
    public int OrderId { get; set; }
    public string? BankCode { get; set; }
    public string? Language { get; set; }
}