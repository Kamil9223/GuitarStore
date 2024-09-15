using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<CategoryId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<CategoryId>))]
public readonly record struct CategoryId(Guid Value) : IStronglyTypedId
{
    public static CategoryId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
