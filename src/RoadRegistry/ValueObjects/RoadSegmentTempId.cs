namespace RoadRegistry.ValueObjects;

using System;
using System.Globalization;

public readonly struct RoadSegmentTempId : IEquatable<RoadSegmentTempId>, IComparable<RoadSegmentTempId>
{
    private readonly int _value;

    public RoadSegmentTempId(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "The road link identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value > 0;
    }

    public bool Equals(RoadSegmentTempId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentTempId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int ToInt()
    {
        return _value;
    }

    public int CompareTo(RoadSegmentTempId other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(RoadSegmentTempId left, RoadSegmentTempId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentTempId left, RoadSegmentTempId right)
    {
        return !left.Equals(right);
    }
}
