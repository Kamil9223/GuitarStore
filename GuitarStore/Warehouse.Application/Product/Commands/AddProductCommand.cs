using Application;

namespace Warehouse.Application.Product.Commands;

public class AddProductCommand : ICommand
{
    public string? Brand { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int CategoryId { get; set; }
}
