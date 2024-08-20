using Domain;

namespace Catalog.Domain;

public class Variation : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public ICollection<Category> Categories { get; private set; } = null!;
    public ICollection<VariationOption> VariationOptions { get; private set; } = null!;
}
