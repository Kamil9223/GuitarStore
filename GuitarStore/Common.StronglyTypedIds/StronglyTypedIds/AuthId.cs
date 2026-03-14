using System.ComponentModel;
using System.Text.Json.Serialization;
using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;

namespace Common.StronglyTypedIds.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<AuthId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<AuthId>))]
public readonly record struct AuthId(Guid Value) : IStronglyTypedId
{
    public static AuthId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();

    public static implicit operator AuthId(Guid value) => new(value);
    public static implicit operator Guid(AuthId id) => id.Value;
}
