using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using BE_DA_WEB.Data;
using BE_DA_WEB.Models;
public interface ICategoryService
{
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category?> UpdateCategoryAsync(int id, Category category);
    Task<bool> DeleteCategoryAsync(int id);
}
public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        // Lấy tất cả danh mục, chỉ cần Name và Description
        return await _context.Categories
                             .AsNoTracking()
                             .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        // Kiểm tra tồn tại theo Name
        var exists = await _context.Categories.AnyAsync(c => c.Name == category.Name);
        if (exists)
            throw new ArgumentException("Danh mục đã tồn tại!");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(int id, Category category)
    {
        var existingCategory = await _context.Categories.FindAsync(id);
        if (existingCategory == null)
            return null;

        existingCategory.Name = category.Name ?? existingCategory.Name;
        existingCategory.Description = category.Description ?? existingCategory.Description;

        await _context.SaveChangesAsync();
        return existingCategory;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
