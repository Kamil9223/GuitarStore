using Domain;
using Domain.StronglyTypedIds;

namespace Catalog.Domain;

public class VariationOption : Entity
{
    public VariationOptionId Id { get; init; }
    public string Value { get; init; } = null!;
    public Variation Variation { get; init; } = null!;
    public ICollection<Product> Products { get; init; } = null!;
}
