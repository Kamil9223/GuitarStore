namespace Warehouse.Application.Abstractions;

internal interface IValidationService<TCommand>
{
    void Validate(TCommand command);
}
