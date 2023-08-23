using Application.RabbitMq.Abstractions;
using Customers.Application.Products.Messages.Events.Incoming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.Application.Products.Handlers.EventHandlers;

internal class ProductAddedEventHandler : IIntegrationEventHandler<ProductAddedEvent>
{
    public async Task Handle(ProductAddedEvent @event)
    {
        
    }
}
