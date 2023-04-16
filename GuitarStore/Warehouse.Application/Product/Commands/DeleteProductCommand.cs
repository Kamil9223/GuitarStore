using Application;

namespace Warehouse.Application.Product.Commands;

public class DeleteProductCommand : ICommand
{
    public int Id { get; set; }
}
