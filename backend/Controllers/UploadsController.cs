using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class uploadsController : ControllerBase
{
    // Nếu muốn lưu ngoài wwwroot, dùng thư mục uploads ở cùng thư mục app chạy
    private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

    [HttpPost]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageDto dto)
    {
        var image = dto.Image;
        if (image == null || image.Length == 0)
        return BadRequest("Không có file nào được tải lên!");

        // Tạo thư mục lưu ảnh nếu chưa tồn tại
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }

        // Tạo tên file duy nhất, ví dụ dùng timestamp + đuôi file
        var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Path.GetExtension(image.FileName)}";

        // Đường dẫn đầy đủ lưu file
        var filePath = Path.Combine(_uploadFolder, fileName);

        // Lưu file vào ổ đĩa
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        // Trả về đường dẫn ảnh (tùy bạn xây dựng URL phù hợp với frontend)
        var imageUrl = $"/uploads/{fileName}";

        return Ok(new { imageUrl });
    }
    public class UploadImageDto
    {
        public IFormFile Image { get; set; } = null!;
    }
}
