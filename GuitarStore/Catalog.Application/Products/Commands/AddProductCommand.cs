using Application;

namespace Catalog.Application.Products.Commands;

public class AddProductCommand : ICommand
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int CategoryId { get; init; }
    public int BrandId { get; init; }
    public ICollection<int> VariationOptionIds { get; init; } = null!;
}
