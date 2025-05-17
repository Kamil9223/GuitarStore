using Domain;
using Domain.StronglyTypedIds;

namespace Catalog.Domain;

public class Category : Entity
{
    public CategoryId Id { get; init; }
    public string CategoryName { get; init; } = null!;
    public Category ParentCategory { get; init; } = null!;
    public CategoryId? ParentCategoryId { get; init; }
    public ICollection<Category> SubCategories { get; init; } = null!;
    public ICollection<Product> Products { get; init; } = null!;
    public ICollection<Variation> Variations { get; init; } = null!;
}
