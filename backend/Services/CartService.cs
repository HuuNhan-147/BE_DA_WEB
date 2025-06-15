using BE_DA_WEB.Data;
using BE_DA_WEB.Models;
using Microsoft.EntityFrameworkCore;
using BE_DA_WEB.Dtos;
public interface ICartService
{
    Task<(bool IsSuccess, string Message)> AddToCartAsync(int userId, int productId, int quantity);
    Task<CartResponse?> GetCartAsync(int userId);
    Task<(bool IsSuccess, string Message)> UpdateCartItemAsync(int userId, int productId, int quantity);
    Task<(bool IsSuccess, string Message)> RemoveFromCartAsync(int userId, int productId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsSuccess, string Message)> AddToCartAsync(int userId, int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return (false, "Sản phẩm không tồn tại!");

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>()
            };
            _context.Carts.Add(cart);
        }

        var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity += quantity;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Image = product.Image,
                Price = product.Price,
                Quantity = quantity
            });
        }

        cart.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return (true, "Đã thêm vào giỏ hàng!");
    }

    public async Task<CartResponse?> GetCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(i => i.Product)
            .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.CartItems.Count == 0)
            return null;

        var validItems = cart.CartItems
            .Where(i => i.Product != null)
            .Select(i => new CartItemDto
            {
                Product = new ProductDto
                {
                    Id = i.Product.Id,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    Image = i.Product.Image,
                    CountInStock = i.Product.CountInStock,
                    Description = i.Product.Description,
                    Rating = i.Product.Rating,
                    CategoryId = i.Product.CategoryId,
                    CategoryName = i.Product.Category != null ? i.Product.Category.Name : null
                },
                Quantity = i.Quantity
            })
            .ToList();

        var itemsPrice = validItems.Sum(i => i.Product.Price * i.Quantity);
        var shippingPrice = 30000;
        var taxPrice = itemsPrice * 0.1m;
        var totalPrice = itemsPrice + shippingPrice + taxPrice;

        return new CartResponse
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            CartItems = validItems,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            ItemsPrice = itemsPrice,
            ShippingPrice = shippingPrice,
            TaxPrice = taxPrice,
            TotalPrice = totalPrice
        };
    }
    public async Task<(bool IsSuccess, string Message)> UpdateCartItemAsync(int userId, int productId, int quantity)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return (false, "Giỏ hàng trống!");

        var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            return (false, "Sản phẩm không có trong giỏ!");

        if (quantity <= 0)
        {
            cart.CartItems.Remove(item); // Xóa sản phẩm nếu quantity <= 0
        }
        else
        {
            item.Quantity = quantity;
        }

        cart.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return (true, "Đã cập nhật giỏ hàng!");
    }
    public async Task<(bool IsSuccess, string Message)> RemoveFromCartAsync(int userId, int productId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return (false, "Giỏ hàng không tồn tại!");

        var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            return (false, "Sản phẩm không có trong giỏ!");

        cart.CartItems.Remove(item);

        if (!cart.CartItems.Any())
        {
            _context.Carts.Remove(cart); // Xóa giỏ hàng nếu không còn item
        }
        else
        {
            cart.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return (true, cart.CartItems.Any() ? "Đã xóa sản phẩm khỏi giỏ hàng!" : "Giỏ hàng hiện đã trống!");
    }
}
public class CartItemDto
{
    public ProductDto Product { get; set; } = null!;
    public int Quantity { get; set; }
}

public class CartResponse
{
    public int CartId { get; set; }
    public int UserId { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public decimal ItemsPrice { get; set; }
    public decimal ShippingPrice { get; set; }
    public decimal TaxPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
