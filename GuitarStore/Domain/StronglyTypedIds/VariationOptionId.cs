using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<VariationOptionId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<VariationOptionId>))]
public readonly record struct VariationOptionId(Guid Value) : IStronglyTypedId
{
    public static VariationOptionId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
