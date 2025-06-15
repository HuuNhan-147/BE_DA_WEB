namespace BE_DA_WEB.Dtos;

public class CreateOrderRequestDto
{
    public List<OrderItemDto> OrderItems { get; set; } = new();
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
}