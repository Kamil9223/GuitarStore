namespace Domain.StronglyTypedIds;
public readonly record struct CartItemId(Guid Value)
{
    public static CartItemId New() => new(Guid.NewGuid());
}
