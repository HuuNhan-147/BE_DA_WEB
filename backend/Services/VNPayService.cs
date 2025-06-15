using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using System.Web;
using BE_DA_WEB.Models;
using BE_DA_WEB.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class VNPayService
{
    private readonly VNPaySettings _vnPaySettings;
    private readonly ApplicationDbContext _context;

    public VNPayService(IOptions<VNPaySettings> options, ApplicationDbContext context)
    {
        _vnPaySettings = options.Value;
        _context = context;
    }

    public async Task<string> CreatePaymentUrlAsync(int orderId, string? bankCode, string? language, string clientIp)
{
    var order = await _context.Orders.FindAsync(orderId);
    if (order == null) throw new Exception("Đơn hàng không tồn tại!");

    var amount = ((long)(order.TotalPrice * 100)).ToString();
    var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");

    bankCode = string.IsNullOrEmpty(bankCode) ? "NCB" : bankCode;
    language = string.IsNullOrEmpty(language) ? "vn" : language;

    var vnp_Params = new Dictionary<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", _vnPaySettings.TmnCode },
        { "vnp_Locale", language },
        { "vnp_CurrCode", "VND" },
        { "vnp_TxnRef", orderId.ToString() },
        { "vnp_OrderInfo", $"Thanh toan cho ma GD: {orderId}" },
        { "vnp_OrderType", "other" },
        { "vnp_Amount", amount },
        { "vnp_ReturnUrl", _vnPaySettings.ReturnUrl },
        { "vnp_IpAddr", clientIp },
        { "vnp_CreateDate", createDate },
        { "vnp_BankCode", bankCode }
    };

    var sortedParams = vnp_Params
        .Where(kv => !string.IsNullOrEmpty(kv.Value))
        .OrderBy(kv => kv.Key)
        .ToDictionary(kv => kv.Key, kv => kv.Value);

    var query = new StringBuilder();
    foreach (var kv in sortedParams)
    {
        query.Append($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}&");
    }

    var signData = query.ToString().TrimEnd('&');
    var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_vnPaySettings.HashSecret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
    var vnp_SecureHash = BitConverter.ToString(hash).Replace("-", "").ToLower();

    return $"{_vnPaySettings.Url}?{signData}&vnp_SecureHash={vnp_SecureHash}";
}

    public async Task<object> VNPayReturnAsync(IQueryCollection vnp_Params)
    {
        var vnp_SecureHash = vnp_Params["vnp_SecureHash"];
        var vnp_TxnRef = vnp_Params["vnp_TxnRef"];
        var vnp_ResponseCode = vnp_Params["vnp_ResponseCode"];
        var vnp_TransactionNo = vnp_Params["vnp_TransactionNo"];
        var vnp_Amount = int.Parse(vnp_Params["vnp_Amount"]) / 100;

        var sortedParams = vnp_Params
            .Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
            .OrderBy(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

        var signData = string.Join("&", sortedParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_vnPaySettings.HashSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
        var computedHash = BitConverter.ToString(hash).Replace("-", "").ToLower();

        if (computedHash != vnp_SecureHash.ToString().ToLower())
            throw new Exception("Invalid checksum");

        var order = await _context.Orders.FindAsync(vnp_TxnRef);
        if (order == null) throw new Exception("Order not found");

        if (vnp_ResponseCode == "00")
        {
            order.IsPaid = true;
            order.PaidAt = DateTime.Now;
            order.PaymentStatus = "paid";
            order.PaymentResult = $"Success | TranId: {vnp_TransactionNo}";
        }
        else
        {
            order.PaymentStatus = "failed";
            order.PaymentResult = $"Failed | Code: {vnp_ResponseCode}";
        }

        await _context.SaveChangesAsync();

        return new
        {
            status = order.PaymentStatus,
            orderId = order.Id,
            amount = vnp_Amount,
            transactionId = vnp_TransactionNo
        };
    }
}
