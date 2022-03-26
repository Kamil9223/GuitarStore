namespace Warehouse.Application;

internal interface IValidationService<TCommand>
{
    void Validate(TCommand command);
}
