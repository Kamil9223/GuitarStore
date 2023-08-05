using Domain;

namespace Catalog.Domain;

public class Category : Entity, IIdentifiable
{
    public int Id { get; private set; }
    public string CategoryName { get; private set; } = null!;
    public Category ParentCategory { get; private set; } = null!;
    public int? ParentCategoryId { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = null!;
    public ICollection<Product> Products { get; private set; } = null!;
    public ICollection<Variation> Variations { get; private set; } = null!;
}
