using Microsoft.AspNetCore.Mvc;
using BE_DA_WEB.Models;
using BE_DA_WEB.Filters;
using BE_DA_WEB.Dtos;
[ApiController]
[Route("api/[controller]")]
public class productsController : ControllerBase
{
    private readonly IProductService _productService;
    public productsController(IProductService productService)
    {
        _productService = productService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync(Request);
        return Ok(products);
    }
    // 2. Lấy sản phẩm theo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id, Request);
        if (product == null)
            return NotFound(new { message = "Sản phẩm không tồn tại!" });

        return Ok(product);
    }

    // 3. Thêm sản phẩm (Admin)
    [AdminOnly]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromForm] ProductDto dto, IFormFile? image)
    {
        var (isSuccess, message, product) = await _productService.CreateProductAsync(dto, image, Request);
        if (!isSuccess)
            return BadRequest(new { message });

        return CreatedAtAction(nameof(GetProductById), new { id = product!.Id }, new
        {
            message,
            product = new
            {               
                product.Name,
                product.Price,
                product.Image,
                product.CategoryId,
                product.Rating,
                product.CountInStock,
                product.Description
            }
        });
    }

    // 4. Cập nhật sản phẩm (Admin)
    [AdminOnly]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto dto, IFormFile? image)
    {
        var (isSuccess, message, product) = await _productService.UpdateProductAsync(id, dto, image, Request);
        if (!isSuccess)
            return BadRequest(new { message });

        return Ok(new
        {
            message,
            product = new
            {
                product!.Id,
                product.Name,
                product.Price,
                product.Image,
                product.CategoryId,
                product.Rating,
                product.CountInStock,
                product.Description
            }
        });
    }

    // 5. Xóa sản phẩm (Admin)
    [AdminOnly]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var (isSuccess, message) = await _productService.DeleteProductAsync(id);
        if (!isSuccess)
            return NotFound(new { message });

        return Ok(new { message });
    }
    [HttpGet("search")]
    public async Task<IActionResult> FilterProducts(
        [FromQuery] string? keyword,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] double? rating,
        [FromQuery] string? sortBy)
    {
        var products = await _productService.FilterProductsAsync(
            keyword, categoryId, minPrice, maxPrice, rating, sortBy, Request);

        return Ok(products);
    }
}