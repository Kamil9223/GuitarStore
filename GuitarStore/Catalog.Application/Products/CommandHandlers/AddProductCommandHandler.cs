using Catalog.Application.Abstractions;
using Catalog.Application.Products.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Products.CommandHandlers;

internal class AddProductCommandHandler : ICommandHandler<AddProductCommand>
{
    public async Task Handle(AddProductCommand command)
    {
        
    }
}
