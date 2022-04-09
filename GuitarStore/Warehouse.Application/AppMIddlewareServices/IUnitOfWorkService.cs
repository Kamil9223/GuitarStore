namespace Warehouse.Application.AppMIddlewareServices;

internal interface IUnitOfWorkService
{
    Task Commit();
}
