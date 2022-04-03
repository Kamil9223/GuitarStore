using Application;
using FluentValidation;
using System.Text;
using ValidationException = Application.Exceptions.ValidationException;

namespace Warehouse.Application.AppMIddlewareServices;

internal class ValidationService<TCommand> : IValidationService<TCommand>
    where TCommand : ICommand
{
    private readonly IValidator<TCommand> _validator;

    public ValidationService(IValidator<TCommand> validator)
    {
        _validator = validator;
    }

    public void Validate(TCommand command)
    {
        var validationResult = _validator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errorBuilder = new StringBuilder();

            errorBuilder.AppendLine("Invalid command, reason: ");

            foreach (var error in validationResult.Errors)
            {
                errorBuilder.AppendLine(error.ErrorMessage);
            }

            throw new ValidationException(errorBuilder.ToString());
        }
    }
}
