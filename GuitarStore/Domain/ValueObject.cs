namespace Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public bool Equals(ValueObject? other)
    {
        return Equals(other as object);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, null))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (this.GetType() != obj.GetType())
            return false;

        return this.GetType()
                   .GetProperties()
                   .All(p => Equals(p.GetValue(this, null), p.GetValue(obj, null)));
    }

    public override int GetHashCode()
    {
        int result = 0;

        var properties = this.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(this, null);
            var currentHash = value?.GetHashCode() ?? 0;
            result += 23 + currentHash;
        }

        return result;
    }

    public static bool operator ==(ValueObject obj1, ValueObject obj2)
    {
        if (Equals(obj1, null))
        {
            if (Equals(obj2, null))
            {
                return true;
            }

            return false;
        }

        return obj1.Equals(obj2);
    }

    public static bool operator !=(ValueObject obj1, ValueObject obj2)
    {
        return !(obj1 == obj2);
    }
}
