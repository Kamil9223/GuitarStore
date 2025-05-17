using Domain;
using Domain.StronglyTypedIds;

namespace Catalog.Domain;

public class Variation : Entity
{
    public VariationId Id { get; init; }
    public string Name { get; init; } = null!;
    public ICollection<Category> Categories { get; init; } = null!;
    public ICollection<VariationOption> VariationOptions { get; init; } = null!;
}
