using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<CartItemId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<CartItemId>))]
public readonly record struct CartItemId(Guid Value) : IStronglyTypedId
{
    public static CartItemId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
