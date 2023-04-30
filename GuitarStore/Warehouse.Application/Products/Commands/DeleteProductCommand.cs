using Application;

namespace Warehouse.Application.Products.Commands;

public class DeleteProductCommand : ICommand
{
    public int Id { get; init; }
}
