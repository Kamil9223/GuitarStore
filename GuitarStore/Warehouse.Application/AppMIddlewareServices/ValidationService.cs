using Application;
using FluentValidation;
using System.Text;
using Warehouse.Application.Abstractions;
using ValidationException = Application.Exceptions.ValidationException;

namespace Warehouse.Application.AppMIddlewareServices;

internal class ValidationService<TCommand> : IValidationService<TCommand>
    where TCommand : ICommand
{
    private readonly IList<IValidator<TCommand>> _validators;

    public ValidationService(IList<IValidator<TCommand>> validators)
    {
        _validators = validators;
    }

    public void Validate(TCommand command)
    {
        var validator = _validators.SingleOrDefault();
        if (validator is null)
            return;

        var validationResult = validator.Validate(command);
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
