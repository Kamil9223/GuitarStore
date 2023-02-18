using Application;

namespace Warehouse.Application.Store.commands;

public class DeleteGuitarStoreCommand : ICommand
{
    public int Id { get; init; }
}
