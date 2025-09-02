using Domain.StronglyTypedIds.Helpers;

namespace Common.Errors.Exceptions;

public class NotFoundException : GuitarStoreApplicationException
{
    public NotFoundException(IStronglyTypedId resourceId)
        : base(
            title: "Not_Found",
            applicationErrorCode: ApplicationErrorCode.ResourceNotFound.Format(resourceId))
    { }
}
