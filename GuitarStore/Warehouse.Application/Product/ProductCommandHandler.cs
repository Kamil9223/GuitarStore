using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Product.Commands;

namespace Warehouse.Application.Product;

internal class ProductCommandHandler :
    ICommandHandler<AddProductCommand>,
    ICommandHandler<DeleteProductCommand>,
    ICommandHandler<UpdateProductCommand>
{
    public Task Handle(AddProductCommand command)
    {
        throw new NotImplementedException();
    }

    public Task Handle(DeleteProductCommand command)
    {
        throw new NotImplementedException();
    }

    public Task Handle(UpdateProductCommand command)
    {
        throw new NotImplementedException();
    }
}
