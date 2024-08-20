namespace Domain.StronglyTypedIds;
public sealed record CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
}
