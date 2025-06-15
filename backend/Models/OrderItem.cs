
namespace BE_DA_WEB.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Image { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}