using Microsoft.AspNetCore.Mvc;
using BE_DA_WEB.Dtos;
[Route("api/[controller]")]
[ApiController]
public class VNPayController : ControllerBase
{
    private readonly VNPayService _vnpayService;

    public VNPayController(VNPayService vnpayService)
    {
        _vnpayService = vnpayService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto dto)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var url = await _vnpayService.CreatePaymentUrlAsync(dto.OrderId, dto.BankCode, dto.Language, clientIp);
        return Ok(url);
    }

    [HttpGet("return")]
    public async Task<IActionResult> VNPayReturn()
    {
        try
        {
            var result = await _vnpayService.VNPayReturnAsync(Request.Query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}