using Application.CQRS;

namespace Orders.Application.Orders.Commands.Handlers;
internal class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    public async Task Handle(PlaceOrderCommand command)
    {
        
    }
}
