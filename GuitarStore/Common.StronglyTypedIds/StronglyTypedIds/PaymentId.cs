using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<PaymentId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<PaymentId>))]
public readonly record struct PaymentId(Guid Value) : IStronglyTypedId
{
    public static PaymentId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
