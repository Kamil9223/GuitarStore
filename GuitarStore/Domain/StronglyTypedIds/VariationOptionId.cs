namespace Domain.StronglyTypedIds;
public readonly record struct VariationOptionId(Guid Value)
{
    public static BrandId New() => new(Guid.NewGuid());
}
