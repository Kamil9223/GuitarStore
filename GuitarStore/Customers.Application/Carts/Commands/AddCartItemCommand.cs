using Application.CQRS;

namespace Customers.Application.Carts.Commands;
public class AddCartItemCommand : ICommand
{
    public int ProductId { get; init; }
    public int CustomerId { get; init; }
    public string Name { get; init; }
    public decimal Price { get; init; }
}
