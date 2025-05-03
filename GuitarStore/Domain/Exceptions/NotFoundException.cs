using Domain.StronglyTypedIds.Helpers;

namespace Domain.Exceptions;

public class NotFoundException : GuitarStoreApplicationException
{
    public NotFoundException(IStronglyTypedId resourceId)
        : base($"Resource with Id: [{resourceId}] does not exist.", string.Empty) { }
}
