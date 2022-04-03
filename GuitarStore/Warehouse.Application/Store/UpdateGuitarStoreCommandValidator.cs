using FluentValidation;
using Warehouse.Contracts.Store;

namespace Warehouse.Application.Store;

internal class UpdateGuitarStoreCommandValidator : AbstractValidator<UpdateGuitarStoreCommand>
{
    public UpdateGuitarStoreCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage($"{nameof(UpdateGuitarStoreCommand.Id)} is 0");

        RuleFor(x => x.Name).NotEmpty().WithMessage($"{nameof(UpdateGuitarStoreCommand.Name)} is empty string");
        RuleFor(x => x.Name).MaximumLength(30).WithMessage($"{nameof(UpdateGuitarStoreCommand.Name)} Maximum length is 30");

        RuleFor(x => x.City).NotNull().WithMessage($"{nameof(UpdateGuitarStoreCommand.Name)} is null");
        RuleFor(x => x.City).NotEmpty().WithMessage($"{nameof(UpdateGuitarStoreCommand.City)} is empty string");
        RuleFor(x => x.City).MaximumLength(200).WithMessage($"{nameof(UpdateGuitarStoreCommand.City)} Maximum length is 200");

        RuleFor(x => x.Street).NotNull().WithMessage($"{nameof(UpdateGuitarStoreCommand.Name)} is null");
        RuleFor(x => x.Street).NotEmpty().WithMessage($"{nameof(UpdateGuitarStoreCommand.Street)} is empty string");
        RuleFor(x => x.Street).MaximumLength(400).WithMessage($"{nameof(UpdateGuitarStoreCommand.Street)} Maximum length is 400");

        RuleFor(x => x.PostalCode).NotNull().WithMessage($"{nameof(UpdateGuitarStoreCommand.Name)} is null");
        RuleFor(x => x.PostalCode).NotEmpty().WithMessage($"{nameof(UpdateGuitarStoreCommand.PostalCode)} is empty string");
        RuleFor(x => x.PostalCode).MaximumLength(6).WithMessage($"{nameof(UpdateGuitarStoreCommand.PostalCode)} Maximum length is 6");
    }
}
