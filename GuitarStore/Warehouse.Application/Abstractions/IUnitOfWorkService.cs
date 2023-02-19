namespace Warehouse.Application.Abstractions;

internal interface IUnitOfWorkService
{
    Task Commit();
}
