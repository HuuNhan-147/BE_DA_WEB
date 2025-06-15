namespace BE_DA_WEB.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;
    }
}