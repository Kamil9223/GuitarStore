using System.Text.Json.Serialization;
using System.Text.Json;

namespace Domain.StronglyTypedIds.Helpers;

public class StronglyTypedIdJsonConverter<T> : JsonConverter<T> where T : struct, IStronglyTypedId
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var guidValue = reader.GetString();
        if (Guid.TryParse(guidValue, out var guid))
        {
            return (T)Activator.CreateInstance(typeof(T), guid);
        }

        throw new JsonException($"Unable to convert \"{guidValue}\" to {typeof(T)}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var stronglyTypedId = (IStronglyTypedId)value;
        writer.WriteStringValue(stronglyTypedId.Value.ToString());
    }
}
