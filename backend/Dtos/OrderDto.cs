namespace BE_DA_WEB.Dtos;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public decimal ShippingPrice { get; set; }
    public decimal TaxPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string Status => IsDelivered ? "Delivered" : (IsPaid ? "Paid" : "Pending");
    public List<OrderItemDto> OrderItems { get; set; } = new();
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
}