namespace RoadRegistry.ValueObjects;

using System;

public readonly struct GeometryVersion : IEquatable<GeometryVersion>, IComparable<GeometryVersion>
{
    public static GeometryVersion Initial => new(1);

    private readonly int _value;

    public GeometryVersion(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The geometry version must be greater than or equal to 0.");
        }

        _value = value;
    }

    public int ToInt32()
    {
        return _value;
    }

    public static GeometryVersion? FromValue(int? value)
    {
        if (value is null)
        {
            return null;
        }

        return new GeometryVersion(value.Value);
    }

    public GeometryVersion Next()
    {
        if (_value == int.MaxValue)
        {
            throw new NotSupportedException(
                "There is no next geometry version because the maximum of the integer data type has been reached.");
        }

        return new GeometryVersion(_value + 1);
    }

    public bool Equals(GeometryVersion other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is GeometryVersion version && Equals(version);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public int CompareTo(GeometryVersion other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(GeometryVersion left, GeometryVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GeometryVersion left, GeometryVersion right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(GeometryVersion left, GeometryVersion right)
    {
        return left.CompareTo(right) == -1;
    }

    public static bool operator <=(GeometryVersion left, GeometryVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(GeometryVersion left, GeometryVersion right)
    {
        return left.CompareTo(right) == 1;
    }

    public static bool operator >=(GeometryVersion left, GeometryVersion right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static implicit operator int(GeometryVersion instance)
    {
        return instance._value;
    }
}
