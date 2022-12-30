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

    //For EF Core
    private Category() { }

    private Category(string categoryName, int? parentCategoryId)
    {
        CategoryName = categoryName;
        ParentCategoryId = parentCategoryId;
        Products = new List<Product>();
        SubCategories = new List<Category>();
    }

    public static Category CreateCategory(string categoryName, int? parentCategoryId)
    {
        return new Category(categoryName, parentCategoryId);
    }
}
