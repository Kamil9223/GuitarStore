namespace Catalog.Application.Abstractions;

internal interface IUnitOfWorkService
{
    Task Commit();
}
