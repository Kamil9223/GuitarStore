using Domain.StronglyTypedIds;

namespace Catalog.Domain.IRepositories;

public interface IVariationOptionRepository
{
    Task<ICollection<VariationOption>> Get(IEnumerable<VariationOptionId> Ids, CancellationToken ct);
}
