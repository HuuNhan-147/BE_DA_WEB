namespace BE_DA_WEB.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = $"DH{DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()[^6..]}-{new Random().Next(100, 999)}";
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal ItemsPrice { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal TaxPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime? PaidAt { get; set; }
        public string PaymentStatus { get; set; } = "pending";
        public string? VnPayTransactionId { get; set; }
        public bool IsDelivered { get; set; } = false;
        public DateTime? DeliveredAt { get; set; }
        public ShippingAddress ShippingAddress { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}