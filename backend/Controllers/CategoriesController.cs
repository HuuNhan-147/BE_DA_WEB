using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BE_DA_WEB.Models;
 // Interface ICategoryService
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    // GET: api/categories/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Danh mục không tồn tại!" });

        return Ok(category);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest(new { message = "Tên là bắt buộc!" });

        try
        {
            var created = await _categoryService.CreateCategoryAsync(category);
            return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/categories/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
    {
        var updated = await _categoryService.UpdateCategoryAsync(id, category);
        if (updated == null)
            return NotFound(new { message = "Danh mục không tồn tại!" });

        return Ok(updated);
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await _categoryService.DeleteCategoryAsync(id);
        if (!deleted)
            return NotFound(new { message = "Danh mục không tồn tại!" });

        return Ok(new { message = "Danh mục đã được xóa!" });
    }
}
