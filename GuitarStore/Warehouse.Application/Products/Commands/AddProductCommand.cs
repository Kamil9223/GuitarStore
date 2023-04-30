using Application;

namespace Warehouse.Application.Products.Commands;

public class AddProductCommand : ICommand
{
    public string? Brand { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public int CategoryId { get; init; }
}
