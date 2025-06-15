using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class cartController : ControllerBase
{
    private readonly ICartService _cartService;

    public cartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var userId = int.Parse(User.FindFirst("id")!.Value);
        var (isSuccess, message) = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);
        if (!isSuccess)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = int.Parse(User.FindFirst("id")!.Value);
        var cart = await _cartService.GetCartAsync(userId);
        if (cart == null)
            return NotFound(new { message = "Giỏ hàng trống hoặc không tồn tại!" });

        return Ok(cart);
    }
    [Authorize]
[HttpPut("update")]
public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto dto)
{
    var userId = int.Parse(User.FindFirst("id")!.Value);
    var (isSuccess, message) = await _cartService.UpdateCartItemAsync(userId, dto.ProductId, dto.Quantity);
    if (!isSuccess)
        return NotFound(new { message });

    return Ok(new { message });
}
[Authorize]
[HttpDelete("/{productId}")]
public async Task<IActionResult> RemoveFromCart(int productId)
{
    var userId = int.Parse(User.FindFirst("id")!.Value);
    var (isSuccess, message) = await _cartService.RemoveFromCartAsync(userId, productId);

    if (!isSuccess)
        return NotFound(new { message });

    return Ok(new { message });
}
public class UpdateCartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
}
public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}