using Application.CQRS.Command;
using Common.Errors.Exceptions;
using Domain.StronglyTypedIds;
using Microsoft.EntityFrameworkCore;
using Warehouse.Core.Database;

namespace Warehouse.Core.Commands;
public sealed record IncreaseStockQuantityCommand(ProductId Id, int IncreaseBy) : ICommand;

internal sealed class IncreaseStockQuantityCommandHandler(
    WarehouseDbContext dbContext)
    : ICommandHandler<IncreaseStockQuantityCommand>
{
    public async Task Handle(IncreaseStockQuantityCommand command)
    {
        var productOnStock = await dbContext.Stock
            .FirstOrDefaultAsync(x => x.ProductId == command.Id)
            ?? throw new NotFoundException(command.Id);

        productOnStock.Quantity += command.IncreaseBy;
    }
}
