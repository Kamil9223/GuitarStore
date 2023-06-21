using Application;

namespace Catalog.Application.Products.Commands;

public class DeleteProductCommand : ICommand
{
    public int Id { get; init; }
}
