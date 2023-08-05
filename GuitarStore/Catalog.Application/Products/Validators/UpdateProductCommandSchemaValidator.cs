using Catalog.Application.Products.Commands;
using FluentValidation;

namespace Catalog.Application.Products.Validators;

internal class UpdateProductCommandSchemaValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandSchemaValidator()
    {
        RuleFor(x => x.Id)
           .NotEmpty();

        RuleFor(x => x.Price)
           .NotEmpty();
    }
}
