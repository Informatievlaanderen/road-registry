namespace RoadRegistry.ValueObjects;

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

public readonly struct RoadSegmentVersion : IEquatable<RoadSegmentVersion>, IComparable<RoadSegmentVersion>
{
    public static RoadSegmentVersion Initial => new(1);

    private readonly int _value;

    public RoadSegmentVersion(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The road segment version must be greater than or equal to 0.");
        }

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value >= 0;
    }

    public static RoadSegmentVersion? FromValue(int? value)
    {
        if (value is null)
        {
            return null;
        }

        return new RoadSegmentVersion(value.Value);
    }

    public RoadSegmentVersion Next()
    {
        if (_value == int.MaxValue)
        {
            throw new NotSupportedException(
                "There is no next road segment version because the maximum of the integer data type has been reached.");
        }

        return new RoadSegmentVersion(_value + 1);
    }

    public static RoadSegmentVersion Max(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return new(Math.Max(left._value, right._value));
    }

    public static RoadSegmentVersion Min(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return new(Math.Min(left._value, right._value));
    }

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(RoadSegmentVersion other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentVersion id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(RoadSegmentVersion other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadSegmentVersion instance)
    {
        return instance._value;
    }

    public static bool operator <(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(RoadSegmentVersion left, RoadSegmentVersion right)
    {
        return left.CompareTo(right) >= 0;
    }
}
