using BE_DA_WEB.Data;
using BE_DA_WEB.Models;
using Microsoft.EntityFrameworkCore;
using BE_DA_WEB.Dtos;
public interface IOrderService
{
    Task<(bool isSuccess, string message, Order order)> CreateOrderAsync(int userId, CreateOrderRequestDto dto);
    Task<List<OrderDto>> GetUserOrdersAsync(int userId);
    Task<(bool IsSuccess, string Message)> DeleteOrderAsync(int userId, int orderId, bool isAdmin);
}
public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool isSuccess, string message, Order? order)> CreateOrderAsync(int userId, CreateOrderRequestDto dto)
    {
        if (dto.OrderItems == null || !dto.OrderItems.Any())
        {
            return (false, "Không có sản phẩm nào!", null);
        }

        var orderItems = new List<OrderItem>();
        decimal itemsPrice = 0;

        foreach (var item in dto.OrderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
                return (false, $"Sản phẩm ID {item.ProductId} không tồn tại", null);

            if (product.CountInStock < item.Quantity)
                return (false, $"Sản phẩm '{product.Name}' không đủ hàng trong kho", null);

            // Trừ kho
            product.CountInStock -= item.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Image = product.Image,
                Quantity = item.Quantity,
                Price = product.Price
            };

            itemsPrice += product.Price * item.Quantity;
            orderItems.Add(orderItem);
        }

        var shippingPrice = 30000m;
        var taxPrice = itemsPrice * 0.1m;
        var totalPrice = itemsPrice + shippingPrice + taxPrice;

        var order = new Order
        {
            UserId = userId,
            OrderItems = orderItems,
            ShippingAddress = new ShippingAddress
            {
                FullName = dto.ShippingAddress.FullName,
                Phone = dto.ShippingAddress.Phone,
                Address = dto.ShippingAddress.Address,
                City = dto.ShippingAddress.City,
                Country = dto.ShippingAddress.Country
            },
            PaymentMethod = dto.PaymentMethod,
            ItemsPrice = itemsPrice,
            ShippingPrice = shippingPrice,
            TaxPrice = taxPrice,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Không trả về order đầy đủ để tránh lỗi vòng lặp JSON → chỉ nên trả về Id hoặc map sang DTO đơn giản nếu cần.
        return (true, "Đơn hàng đã được tạo!", order);
    }
    public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingAddress)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var orderDtos = orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            PaymentMethod = order.PaymentMethod,
            ShippingPrice = order.ShippingPrice,
            TaxPrice = order.TaxPrice,
            TotalPrice = order.TotalPrice,
            IsPaid = order.IsPaid,
            PaidAt = order.PaidAt,
            IsDelivered = order.IsDelivered,
            DeliveredAt = order.DeliveredAt,
            OrderItems = order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.Name,
                Price = item.Price,
                Quantity = item.Quantity,
                Image = item.Image
            }).ToList(),
            ShippingAddress = new ShippingAddressDto
            {
                FullName = order.ShippingAddress.FullName,
                Phone = order.ShippingAddress.Phone,
                Address = order.ShippingAddress.Address,
                City = order.ShippingAddress.City,
                Country = order.ShippingAddress.Country
            }
        }).ToList();

        return orderDtos;
    }
    public async Task<(bool IsSuccess, string Message)> DeleteOrderAsync(int userId, int orderId, bool isAdmin)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return (false, "Đơn hàng không tồn tại!");
        }

        // Chỉ cho phép admin hoặc chính chủ đơn hàng xóa
        if (!isAdmin && order.UserId != userId)
        {
            return (false, "Bạn không có quyền xóa đơn hàng này!");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return (true, "Đơn hàng đã được xóa thành công!");
    }
}