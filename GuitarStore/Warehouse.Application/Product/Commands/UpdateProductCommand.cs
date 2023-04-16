using Application;

namespace Warehouse.Application.Product.Commands;

public class UpdateProductCommand : ICommand
{
    public string? Description { get; set; }
    public decimal? Price { get; set; }
}
