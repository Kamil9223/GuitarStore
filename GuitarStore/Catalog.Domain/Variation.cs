using Domain;
using Domain.StronglyTypedIds;

namespace Catalog.Domain;

public class Variation : Entity
{
    public VariationId Id { get; private set; }
    public string Name { get; private set; } = null!;
    public ICollection<Category> Categories { get; private set; } = null!;
    public ICollection<VariationOption> VariationOptions { get; private set; } = null!;
}
