namespace RoadRegistry.BackOffice;

using System;
using System.Globalization;

public readonly struct RoadNodeId : IEquatable<RoadNodeId>, IComparable<RoadNodeId>
{
    private readonly int _value;

    public static readonly RoadNodeId Zero = new(0);

    public RoadNodeId(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "The road node identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value >= 0;
    }

    public RoadNodeId Next()
    {
        if (_value == int.MaxValue)
            throw new NotSupportedException(
                "There is no next road node identifier because the maximum of the integer data type has been reached.");
        return new RoadNodeId(_value + 1);
    }

    public static RoadNodeId Max(RoadNodeId left, RoadNodeId right)
    {
        return new RoadNodeId(Math.Max(left._value, right._value));
    }

    public static RoadNodeId Min(RoadNodeId left, RoadNodeId right)
    {
        return new RoadNodeId(Math.Min(left._value, right._value));
    }

    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(RoadNodeId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadNodeId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(RoadNodeId other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(RoadNodeId left, RoadNodeId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadNodeId left, RoadNodeId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadNodeId instance)
    {
        return instance._value;
    }

    public static bool operator <(RoadNodeId left, RoadNodeId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(RoadNodeId left, RoadNodeId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(RoadNodeId left, RoadNodeId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(RoadNodeId left, RoadNodeId right)
    {
        return left.CompareTo(right) >= 0;
    }
}
