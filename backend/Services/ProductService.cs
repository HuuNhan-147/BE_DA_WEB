using BE_DA_WEB.Data;
using BE_DA_WEB.Models;
using Microsoft.EntityFrameworkCore;
using BE_DA_WEB.Dtos;
public interface IProductService
{
    Task<(bool isSuccess, string message, Product? product)> CreateProductAsync(ProductDto dto, IFormFile? imageFile, HttpRequest request);
    Task<(bool isSuccess, string message, Product? product)> UpdateProductAsync(int id, ProductDto dto, IFormFile? imageFile, HttpRequest request);
    Task<(bool isSuccess, string message)> DeleteProductAsync(int id);
    Task<ProductDto?> GetProductByIdAsync(int id, HttpRequest request);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(HttpRequest request);
     Task<IEnumerable<ProductDto>> FilterProductsAsync(
        string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, double? rating, string? sortBy, HttpRequest request);
}

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(HttpRequest request)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync();

        var hostUrl = $"{request.Scheme}://{request.Host}";

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryName = p.Category?.Name,
            Image = string.IsNullOrEmpty(p.Image) ? "" : hostUrl + p.Image
        });
    }
    public async Task<ProductDto?> GetProductByIdAsync(int id, HttpRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return null;

        string imageUrl = string.Empty;
        if (!string.IsNullOrEmpty(product.Image))
        {
            imageUrl = $"{request.Scheme}://{request.Host}{product.Image}";
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Image = imageUrl,
            CategoryId = product.CategoryId,
            Rating = product.Rating,
            CountInStock = product.CountInStock,
            Description = product.Description
        };
    }

    public async Task<(bool, string, Product?)> CreateProductAsync(ProductDto dto, IFormFile? imageFile, HttpRequest request)
    {
        // Validate category
        var existingCategory = await _context.Categories.FindAsync(dto.CategoryId);
        if (existingCategory == null)
            return (false, "Danh mục không hợp lệ! Vui lòng chọn danh mục có sẵn.", null);

        // Validate required fields
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            dto.Price <= 0 ||
            dto.Rating < 0 ||
            dto.CountInStock < 0 ||
            string.IsNullOrWhiteSpace(dto.Description))
        {
            return (false, "Vui lòng nhập đầy đủ thông tin sản phẩm!", null);
        }

        // Lưu ảnh (nếu có)
        string imagePath = "/uploads/default.jpg";
        if (imageFile != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

            // Đường dẫn tuyệt đối đến thư mục uploads trong thư mục gốc dự án
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            // Gán đường dẫn tương đối để lưu vào CSDL (phục vụ client)
            imagePath = $"/uploads/{fileName}";
        }

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            Category = existingCategory,
            Rating = dto.Rating,
            CountInStock = dto.CountInStock,
            Description = dto.Description,
            Image = imagePath
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Thêm base url vào image
        product.Image = $"{request.Scheme}://{request.Host}{product.Image}";

        return (true, "Sản phẩm đã được thêm thành công!", product);
    }

    public async Task<(bool, string, Product?)> UpdateProductAsync(int id, ProductDto dto, IFormFile? imageFile, HttpRequest request)
    {
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return (false, "Sản phẩm không tồn tại!", null);

        if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name;
        if (dto.Price > 0) product.Price = dto.Price;
        if (dto.Rating >= 0) product.Rating = dto.Rating;
        if (dto.CountInStock >= 0) product.CountInStock = dto.CountInStock;
        if (!string.IsNullOrWhiteSpace(dto.Description)) product.Description = dto.Description;

        if (dto.CategoryId > 0 && dto.CategoryId != product.CategoryId)
        {
            var existingCategory = await _context.Categories.FindAsync(dto.CategoryId);
            if (existingCategory == null)
                return (false, "Danh mục không hợp lệ!", null);

            product.CategoryId = dto.CategoryId;
            product.Category = existingCategory;
        }

        if (imageFile != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            product.Image = $"/uploads/{fileName}";
        }

        await _context.SaveChangesAsync();

        product.Image = $"{request.Scheme}://{request.Host}{product.Image}";

        return (true, "Sản phẩm đã được cập nhật thành công!", product);
    }

    public async Task<(bool, string)> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return (false, "Sản phẩm không tồn tại!");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return (true, "Xóa sản phẩm thành công!");
    }
    public async Task<IEnumerable<ProductDto>> FilterProductsAsync(
        string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, double? rating, string? sortBy, HttpRequest request)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        // Tìm kiếm theo tên
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.Name.Contains(keyword));

        // Lọc theo danh mục
        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        // Lọc theo giá
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        // Lọc theo đánh giá
        if (rating.HasValue)
            query = query.Where(p => p.Rating >= rating.Value);

        // Sắp xếp
        switch (sortBy)
        {
            case "priceLowHigh":
                query = query.OrderBy(p => p.Price);
                break;
            case "priceHighLow":
                query = query.OrderByDescending(p => p.Price);
                break;
            case "latest":
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
            case "bestSelling":
                query = query.OrderByDescending(p => p.NumReviews); // hoặc trường bán chạy nhất nếu có
                break;
            default:
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
        }

        var hostUrl = $"{request.Scheme}://{request.Host}";

        var products = await query.ToListAsync();

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            Image = string.IsNullOrEmpty(p.Image) ? "" : hostUrl + p.Image,
            Rating = p.Rating,
            CountInStock = p.CountInStock
        });
    }
    
}

// Sửa lại ProductDto cho phù hợp với SQL Server
