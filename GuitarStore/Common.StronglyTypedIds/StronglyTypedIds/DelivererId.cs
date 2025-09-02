using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<DelivererId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<DelivererId>))]
public readonly record struct DelivererId(Guid Value) : IStronglyTypedId
{
    public static DelivererId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
