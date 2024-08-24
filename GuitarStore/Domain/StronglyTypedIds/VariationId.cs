namespace Domain.StronglyTypedIds;
public readonly record struct VariationId(Guid Value)
{
    public static BrandId New() => new(Guid.NewGuid());
}
