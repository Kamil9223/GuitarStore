using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<OrderItemId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<OrderItemId>))]
public readonly record struct OrderItemId(Guid Value) : IStronglyTypedId
{
    public static OrderItemId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}