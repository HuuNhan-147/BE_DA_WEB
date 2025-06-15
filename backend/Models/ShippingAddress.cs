namespace BE_DA_WEB.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = null!;
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}