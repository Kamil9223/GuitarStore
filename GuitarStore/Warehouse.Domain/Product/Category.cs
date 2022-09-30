using Domain;

namespace Warehouse.Domain.Product;

public class Category : Entity, IIdentifiable
{
    public int Id { get; }
    public string CategoryName { get; }
    public int? SubCategoryId { get; }
    public Category SubCategory { get; }
    public ICollection<Product> Products { get; }

    //For EF Core
    private Category() { }

    private Category(string categoryName, int? subCategoryId)
    {
        CategoryName = categoryName;
        SubCategoryId = subCategoryId;
        Products = new List<Product>();
    }

    public static Category CreateCategory(string categoryName, int? subCategoryId)
    {
        //Check rules?

        return new Category(categoryName, subCategoryId);
    }
}
