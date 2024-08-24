namespace Domain.StronglyTypedIds;
public readonly record struct BrandId(Guid Value)
{
    public static BrandId New() => new(Guid.NewGuid());
}
