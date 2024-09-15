using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<VariationId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<VariationId>))]
public readonly record struct VariationId(Guid Value) : IStronglyTypedId
{
    public static VariationId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
