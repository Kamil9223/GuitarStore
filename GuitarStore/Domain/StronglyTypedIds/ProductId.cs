namespace Domain.StronglyTypedIds;
public sealed record ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.NewGuid());
}
