namespace Warehouse.Application.AppMIddlewareServices;

internal interface IValidationService<TCommand>
{
    void Validate(TCommand command);
}
