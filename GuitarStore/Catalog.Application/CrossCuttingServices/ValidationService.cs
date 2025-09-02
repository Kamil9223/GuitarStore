using Application.CQRS;
using Catalog.Application.Abstractions;
using FluentValidation;
using System.Text;
using ValidationException = Common.Errors.Exceptions.ValidationException;

namespace Catalog.Application.CrossCuttingServices;

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
