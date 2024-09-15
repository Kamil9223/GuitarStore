﻿using Domain.StronglyTypedIds.Helpers;
using SequentialGuid;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.StronglyTypedIds;

[TypeConverter(typeof(StrongTypeIdConverter<CustomerId>))]
[JsonConverter(typeof(StronglyTypedIdJsonConverter<CustomerId>))]
public readonly record struct CustomerId(Guid Value) : IStronglyTypedId
{
    public static CustomerId New() => new(SequentialGuidGenerator.Instance.NewGuid());

    public override string ToString() => Value.ToString();
}
