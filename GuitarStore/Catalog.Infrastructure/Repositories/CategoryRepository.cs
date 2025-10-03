using Catalog.Domain;
using Catalog.Domain.IRepositories;
using Catalog.Infrastructure.Database;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

internal class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _catalogDbContext;

    public CategoryRepository(CatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<Category?> GetCategoryThatHasNotChildren(CategoryId id, CancellationToken ct)
    {
        return await _catalogDbContext.Categories
            .Include(x => x.SubCategories)
            .Where(x => x.Id == id)
            .Where(x => x.SubCategories.Any() == false)
            .SingleOrDefaultAsync(ct);
    }
}
