using Application;

namespace Warehouse.Application.Store.Commands;

public class DeleteGuitarStoreCommand : ICommand
{
    public int Id { get; init; }
}
