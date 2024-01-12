namespace RoadRegistry.BackOffice;

using System;
using System.Globalization;

public readonly struct StreetNameId : IEquatable<StreetNameId>, IComparable<StreetNameId>
{
    private readonly int _value;

    public StreetNameId(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The street name identifier must be greater than zero.");
        }

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value > 0;
    }

    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(StreetNameId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is StreetNameId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(StreetNameId other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(StreetNameId left, StreetNameId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StreetNameId left, StreetNameId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(StreetNameId instance)
    {
        return instance._value;
    }

    public static bool operator <(StreetNameId left, StreetNameId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(StreetNameId left, StreetNameId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(StreetNameId left, StreetNameId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(StreetNameId left, StreetNameId right)
    {
        return left.CompareTo(right) >= 0;
    }
}
