namespace RoadRegistry.BackOffice;

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

public readonly struct RoadSegmentPosition : IEquatable<RoadSegmentPosition>, IComparable<RoadSegmentPosition>
{
    public static readonly RoadSegmentPosition Zero = new(0.0m);
    private readonly decimal _value;

    public RoadSegmentPosition(decimal value)
    {
        if (value < 0.0m) throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment position must be greater than or equal to zero.");

        _value = value;
    }

    public static RoadSegmentPosition FromDouble(double value)
    {
        return new RoadSegmentPosition(Convert.ToDecimal(value));
    }

    public static bool Accepts(double value)
    {
        return value >= 0.0;
    }

    public static bool Accepts(decimal value)
    {
        return value >= 0.0m;
    }

    [Pure]
    public decimal ToDecimal()
    {
        return _value;
    }

    [Pure]
    public double ToDouble()
    {
        return decimal.ToDouble(_value);
    }

    public bool Equals(RoadSegmentPosition other)
    {
        return _value.Equals(other._value);
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentPosition id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(RoadSegmentPosition other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(RoadSegmentPosition left, RoadSegmentPosition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentPosition left, RoadSegmentPosition right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(RoadSegmentPosition left, RoadSegmentPosition right)
    {
        return left.CompareTo(right) == -1;
    }

    public static bool operator <=(RoadSegmentPosition left, RoadSegmentPosition right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(RoadSegmentPosition left, RoadSegmentPosition right)
    {
        return left.CompareTo(right) == 1;
    }

    public static bool operator >=(RoadSegmentPosition left, RoadSegmentPosition right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static implicit operator decimal(RoadSegmentPosition instance)
    {
        return instance._value;
    }

    public static implicit operator double(RoadSegmentPosition instance)
    {
        return instance.ToDouble();
    }
}
