using Application;

namespace Catalog.Application.Products.Commands;

public class UpdateProductCommand : ICommand
{
    public int Id { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
}
