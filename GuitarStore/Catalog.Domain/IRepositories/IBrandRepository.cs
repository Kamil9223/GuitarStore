using Domain.StronglyTypedIds;

namespace Catalog.Domain.IRepositories;

public interface IBrandRepository
{
    Task<Brand?> Get(BrandId id, CancellationToken ct);
}
