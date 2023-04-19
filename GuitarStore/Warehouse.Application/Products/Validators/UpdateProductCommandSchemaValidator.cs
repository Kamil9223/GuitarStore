using FluentValidation;
using Warehouse.Application.Products.Commands;

namespace Warehouse.Application.Products.Validators;

internal class UpdateProductCommandSchemaValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandSchemaValidator()
    {
        RuleFor(x => x.Id)
           .NotEmpty();

        RuleFor(x => x.Price)
           .NotNull()
           .NotEmpty();
    }
}
