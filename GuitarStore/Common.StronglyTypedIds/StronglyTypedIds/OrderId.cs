using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<OrderId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<OrderId>))]
public readonly record struct OrderId(Guid Value) : IStronglyTypedId
{
    public static OrderId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public static implicit operator OrderId(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}
