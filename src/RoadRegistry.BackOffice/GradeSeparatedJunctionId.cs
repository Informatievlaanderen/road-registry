namespace RoadRegistry.BackOffice;

using System;
using System.Globalization;

public readonly struct GradeSeparatedJunctionId : IEquatable<GradeSeparatedJunctionId>, IComparable<GradeSeparatedJunctionId>
{
    private readonly int _value;

    public GradeSeparatedJunctionId(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "The grade separated junction identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value >= 0;
    }

    public GradeSeparatedJunctionId Next()
    {
        if (_value == int.MaxValue)
            throw new NotSupportedException(
                "There is no next grade separated junction identifier because the maximum of the integer data type has been reached.");
        return new GradeSeparatedJunctionId(_value + 1);
    }

    public static GradeSeparatedJunctionId Max(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return new GradeSeparatedJunctionId(Math.Max(left._value, right._value));
    }

    public static GradeSeparatedJunctionId Min(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return new GradeSeparatedJunctionId(Math.Min(left._value, right._value));
    }

    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(GradeSeparatedJunctionId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is GradeSeparatedJunctionId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(GradeSeparatedJunctionId other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(GradeSeparatedJunctionId instance)
    {
        return instance._value;
    }

    public static bool operator <(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right)
    {
        return left.CompareTo(right) >= 0;
    }
}