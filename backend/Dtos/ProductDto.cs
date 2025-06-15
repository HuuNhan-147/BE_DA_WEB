namespace BE_DA_WEB.Dtos;
public class ProductDto
{


    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }  // Thay vào đây
    public string? Image { get; set; }
    public double Rating { get; set; }
    public int CountInStock { get; set; }
    public string Description { get; set; } = string.Empty;

}