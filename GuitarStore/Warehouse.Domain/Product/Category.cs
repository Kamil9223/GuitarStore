using Domain;

namespace Warehouse.Domain.Product;

public class Category : Entity, IIdentifiable
{
    public int Id { get; }
    public string CategoryName { get; }
    public int? ParentCategoryId { get; }
    public Category ParentCategory { get; }
    public ICollection<Category> SubCategories { get; }
    public ICollection<Product> Products { get; }
}
