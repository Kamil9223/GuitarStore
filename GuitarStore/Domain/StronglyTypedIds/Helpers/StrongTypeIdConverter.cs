using System.ComponentModel;
using System.Globalization;

namespace Domain.StronglyTypedIds.Helpers;
public class StrongTypeIdConverter<StrongTypeIdType> : TypeConverter
    where StrongTypeIdType : struct, IStronglyTypedId
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue && Guid.TryParse(stringValue, out var guid))
        {
            return Activator.CreateInstance(typeof(StrongTypeIdType), guid);
        }
        return base.ConvertFrom(context, culture, value);
    }
}
