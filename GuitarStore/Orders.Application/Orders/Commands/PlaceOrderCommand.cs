using Application.CQRS;

namespace Orders.Application.Orders.Commands;
public class PlaceOrderCommand : ICommand
{
    public int CustomerId { get; init; }
}
