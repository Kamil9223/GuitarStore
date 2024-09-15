using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<ProductId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<ProductId>))]
public readonly record struct ProductId(Guid Value) : IStronglyTypedId
{
    public static ProductId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
