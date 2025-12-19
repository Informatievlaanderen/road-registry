namespace RoadRegistry.ValueObjects;

using System;
using System.Globalization;

public readonly struct RoadSegmentId : IEquatable<RoadSegmentId>, IComparable<RoadSegmentId>
{
    private readonly int _value;

    public RoadSegmentId(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "The road link identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value > 0;
    }

    public RoadSegmentId Next()
    {
        if (_value == int.MaxValue)
            throw new NotSupportedException(
                "There is no next road segment identifier because the maximum of the integer data type has been reached.");
        return new RoadSegmentId(_value + 1);
    }

    public static RoadSegmentId Max(RoadSegmentId left, RoadSegmentId right)
    {
        return new RoadSegmentId(Math.Max(left._value, right._value));
    }

    public static RoadSegmentId Min(RoadSegmentId left, RoadSegmentId right)
    {
        return new RoadSegmentId(Math.Min(left._value, right._value));
    }

    public static RoadSegmentId? FromValue(int? value)
    {
        return value.HasValue
            ? new RoadSegmentId(value.Value)
            : null;
    }

    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(RoadSegmentId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(RoadSegmentId other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(RoadSegmentId left, RoadSegmentId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentId left, RoadSegmentId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadSegmentId instance)
    {
        return instance._value;
    }

    public static bool operator <(RoadSegmentId left, RoadSegmentId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(RoadSegmentId left, RoadSegmentId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(RoadSegmentId left, RoadSegmentId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(RoadSegmentId left, RoadSegmentId right)
    {
        return left.CompareTo(right) >= 0;
    }
}
