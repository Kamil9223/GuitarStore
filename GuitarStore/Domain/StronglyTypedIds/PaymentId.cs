namespace Domain.StronglyTypedIds;
public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId New() => new(Guid.NewGuid());
}
