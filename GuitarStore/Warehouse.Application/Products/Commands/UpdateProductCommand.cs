using Application;

namespace Warehouse.Application.Products.Commands;

public class UpdateProductCommand : ICommand
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
}
