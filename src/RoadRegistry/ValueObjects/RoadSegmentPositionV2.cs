namespace RoadRegistry.ValueObjects;

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

public readonly struct RoadSegmentPositionV2 : IEquatable<RoadSegmentPositionV2>, IComparable<RoadSegmentPositionV2>
{
    public static readonly RoadSegmentPositionV2 Zero = new(0.0);
    private readonly double _value;

    public RoadSegmentPositionV2(double value)
    {
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment position must be greater than or equal to zero.");
        }

        _value = value;
    }

    public static bool Accepts(double value)
    {
        return value >= 0.0;
    }

    [Pure]
    public double ToDouble()
    {
        return _value;
    }

    public bool Equals(RoadSegmentPositionV2 other)
    {
        return _value.Equals(other._value);
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentPositionV2 id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(RoadSegmentPositionV2 other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(RoadSegmentPositionV2 left, RoadSegmentPositionV2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentPositionV2 left, RoadSegmentPositionV2 right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(RoadSegmentPositionV2 left, RoadSegmentPositionV2 right)
    {
        return left.CompareTo(right) == -1;
    }

    public static bool operator <=(RoadSegmentPositionV2 left, RoadSegmentPositionV2 right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(RoadSegmentPositionV2 left, RoadSegmentPositionV2 right)
    {
        return left.CompareTo(right) == 1;
    }

    public static bool operator >=(RoadSegmentPositionV2 left, RoadSegmentPositionV2 right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static implicit operator double(RoadSegmentPositionV2 instance)
    {
        return instance._value;
    }
}
