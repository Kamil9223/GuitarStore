using Catalog.Application.Products.Commands;
using FluentValidation;

namespace Catalog.Application.Products.Validators;

internal class AddProductCommandSchemaValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandSchemaValidator()
    {
        RuleFor(x => x.Brand)
            .NotNull()
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
           .NotNull()
           .NotEmpty();

        RuleFor(x => x.CategoryId)
           .NotEmpty();
    }
}
