namespace BE_DA_WEB.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public string Phone { get; set; } = string.Empty;
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpires { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}