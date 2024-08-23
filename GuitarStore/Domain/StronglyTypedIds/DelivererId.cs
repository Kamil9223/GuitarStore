namespace Domain.StronglyTypedIds;
public readonly record struct DelivererId(Guid Value)
{
    public static DelivererId New() => new(Guid.NewGuid());
}
