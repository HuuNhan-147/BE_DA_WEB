using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BE_DA_WEB.Dtos;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto dto)
    {
        // Lấy userId từ JWT claims
        var userIdClaim = User.FindFirst("id");
        if (userIdClaim == null)
            return Unauthorized(new { message = "Không xác định được người dùng" });

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return BadRequest(new { message = "ID người dùng không hợp lệ" });

        var (isSuccess, message, order) = await _orderService.CreateOrderAsync(userId, dto);

        if (!isSuccess)
            return BadRequest(new { message });

        // Trả về thông tin đơn hàng cần thiết (không trả toàn bộ Order để tránh lỗi vòng lặp JSON)
        return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, new
        {
            message,
            orderId = order.Id,
            totalPrice = order.TotalPrice,
            createdAt = order.CreatedAt
        });
    }
    [HttpGet("me")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userIdClaim = User.FindFirst("id");
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "Không có quyền truy cập!" });
        }

        var userId = int.Parse(userIdClaim.Value);
        var orders = await _orderService.GetUserOrdersAsync(userId);

        if (orders.Count == 0)
        {
            return NotFound(new { message = "Không có đơn hàng nào!" });
        }

        return Ok(new
        {
            message = "Lấy danh sách đơn hàng của người dùng thành công!",
            orders
        });
    }
     [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var userId = int.Parse(User.FindFirst("id")!.Value);
        var isAdmin = bool.Parse(User.FindFirst("isAdmin")!.Value);

        var (isSuccess, message) = await _orderService.DeleteOrderAsync(userId, id, isAdmin);

        if (!isSuccess)
            return BadRequest(new { message });

        return Ok(new { message });
    }
}
