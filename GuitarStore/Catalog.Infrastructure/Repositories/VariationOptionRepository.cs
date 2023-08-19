using Catalog.Domain;
using Catalog.Domain.IRepositories;
using Catalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories;

internal class VariationOptionRepository : IVariationOptionRepository
{
    private readonly CatalogDbContext _catalogDbContext;

    public VariationOptionRepository(CatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<ICollection<VariationOption>> Get(IEnumerable<int> Ids)
    {
        return await _catalogDbContext.VariationOptions
            .Where(x => Ids.Contains(x.Id))
            .ToListAsync();
    }
}
