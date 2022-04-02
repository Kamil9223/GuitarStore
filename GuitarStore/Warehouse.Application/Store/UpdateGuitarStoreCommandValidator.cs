using FluentValidation;
using Warehouse.Contracts.Store;

namespace Warehouse.Application.Store;

internal class UpdateGuitarStoreCommandValidator : AbstractValidator<UpdateGuitarStoreCommand>
{
    public UpdateGuitarStoreCommandValidator()
    {

    }
}
