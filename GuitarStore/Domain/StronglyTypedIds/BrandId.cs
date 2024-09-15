using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<BrandId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<BrandId>))]
public readonly record struct BrandId(Guid Value) : IStronglyTypedId
{
    public static BrandId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
