namespace BE_DA_WEB.Dtos;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public string Image { get; set; } = null!;
}
