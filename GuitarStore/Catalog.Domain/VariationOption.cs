using Domain;
using Domain.StronglyTypedIds;

namespace Catalog.Domain;

public class VariationOption : Entity
{
    public VariationOptionId Id { get; private set; }
    public string Value { get; private set; } = null!;
    public Variation Variation { get; private set; } = null!;
    public ICollection<Product> Products { get; private set; } = null!;
}
