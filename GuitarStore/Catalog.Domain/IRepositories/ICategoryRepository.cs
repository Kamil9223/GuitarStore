using Domain.StronglyTypedIds;

namespace Catalog.Domain.IRepositories;

public interface ICategoryRepository
{
    Task<Category?> GetCategoryThatHasNotChildren(CategoryId id, CancellationToken ct);
}
