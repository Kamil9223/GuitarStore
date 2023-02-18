using FluentValidation;
using Warehouse.Application.Store.commands;

namespace Warehouse.Application.Store;

internal class AddGuitarStoreCommandValidator : AbstractValidator<AddGuitarStoreCommand>
{
    public AddGuitarStoreCommandValidator()
    {
        RuleFor(x => x.Name).NotNull().WithMessage($"{nameof(AddGuitarStoreCommand.Name)} is null");
        RuleFor(x => x.Name).NotEmpty().WithMessage($"{nameof(AddGuitarStoreCommand.Name)} is empty string");
        RuleFor(x => x.Name).MaximumLength(30).WithMessage($"{nameof(AddGuitarStoreCommand.Name)} Maximum length is 30");

        RuleFor(x => x.City).NotNull().WithMessage($"{nameof(AddGuitarStoreCommand.City)} is null");
        RuleFor(x => x.City).NotEmpty().WithMessage($"{nameof(AddGuitarStoreCommand.City)} is empty string");
        RuleFor(x => x.City).MaximumLength(200).WithMessage($"{nameof(AddGuitarStoreCommand.City)} Maximum length is 200");

        RuleFor(x => x.Street).NotNull().WithMessage($"{nameof(AddGuitarStoreCommand.Street)} is null");
        RuleFor(x => x.Street).NotEmpty().WithMessage($"{nameof(AddGuitarStoreCommand.Street)} is empty string");
        RuleFor(x => x.Street).MaximumLength(400).WithMessage($"{nameof(AddGuitarStoreCommand.Street)} Maximum length is 400");

        RuleFor(x => x.PostalCode).NotNull().WithMessage($"{nameof(AddGuitarStoreCommand.PostalCode)} is null");
        RuleFor(x => x.PostalCode).NotEmpty().WithMessage($"{nameof(AddGuitarStoreCommand.PostalCode)} is empty string");
        RuleFor(x => x.PostalCode).MaximumLength(6).WithMessage($"{nameof(AddGuitarStoreCommand.PostalCode)} Maximum length is 6");
    }
}
