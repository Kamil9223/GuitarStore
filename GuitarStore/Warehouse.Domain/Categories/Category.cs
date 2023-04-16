using Domain;
using Warehouse.Domain.Products;

namespace Warehouse.Domain.Categories;

public class Category : Entity, IIdentifiable
{
    public int Id { get; }
    public string? CategoryName { get; }
    public Category? ParentCategory { get; }
    public ICollection<Category>? SubCategories { get; }
    public ICollection<Product>? Products { get; }
}
