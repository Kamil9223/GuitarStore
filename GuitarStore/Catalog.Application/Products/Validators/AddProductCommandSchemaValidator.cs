using Catalog.Application.Products.Commands;
using FluentValidation;

namespace Catalog.Application.Products.Validators;

internal class AddProductCommandSchemaValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandSchemaValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.CategoryId)
           .NotEmpty();

        RuleFor(x => x.BrandId)
           .NotEmpty();

        RuleFor(x => x.VariationOptionIds)
           .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}
