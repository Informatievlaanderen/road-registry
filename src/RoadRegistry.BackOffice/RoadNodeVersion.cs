namespace RoadRegistry.BackOffice;

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

public readonly struct RoadNodeVersion : IEquatable<RoadNodeVersion>, IComparable<RoadNodeVersion>
{
    public static RoadNodeVersion Initial => new(1);
    
    private readonly int _value;

    public RoadNodeVersion(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The road node version must be greater than or equal to 0.");
        }

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value >= 0;
    }

    public RoadNodeVersion Next()
    {
        if (_value == int.MaxValue)
        {
            throw new NotSupportedException(
                "There is no next road segment version because the maximum of the integer data type has been reached.");
        }

        return new RoadNodeVersion(_value + 1);
    }

    public static RoadNodeVersion Max(RoadNodeVersion left, RoadNodeVersion right)
    {
        return new(Math.Max(left._value, right._value));
    }

    public static RoadNodeVersion Min(RoadNodeVersion left, RoadNodeVersion right)
    {
        return new(Math.Min(left._value, right._value));
    }

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(RoadNodeVersion other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadNodeVersion id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(RoadNodeVersion other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(RoadNodeVersion left, RoadNodeVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadNodeVersion left, RoadNodeVersion right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadNodeVersion instance)
    {
        return instance._value;
    }

    public static bool operator <(RoadNodeVersion left, RoadNodeVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(RoadNodeVersion left, RoadNodeVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(RoadNodeVersion left, RoadNodeVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(RoadNodeVersion left, RoadNodeVersion right)
    {
        return left.CompareTo(right) >= 0;
    }
}
