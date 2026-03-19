namespace RoadRegistry.ValueObjects;

using System;
using System.Globalization;

public readonly struct GradeJunctionId : IEquatable<GradeJunctionId>, IComparable<GradeJunctionId>
{
    private readonly int _value;

    public GradeJunctionId(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "The grade junction identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value >= 0;
    }

    public GradeJunctionId Next()
    {
        if (_value == int.MaxValue)
            throw new NotSupportedException(
                "There is no next grade junction identifier because the maximum of the integer data type has been reached.");
        return new GradeJunctionId(_value + 1);
    }

    public static GradeJunctionId Max(GradeJunctionId left, GradeJunctionId right)
    {
        return new GradeJunctionId(Math.Max(left._value, right._value));
    }

    public static GradeJunctionId Min(GradeJunctionId left, GradeJunctionId right)
    {
        return new GradeJunctionId(Math.Min(left._value, right._value));
    }

    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(GradeJunctionId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is GradeJunctionId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(GradeJunctionId other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(GradeJunctionId left, GradeJunctionId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GradeJunctionId left, GradeJunctionId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(GradeJunctionId instance)
    {
        return instance._value;
    }

    public static bool operator <(GradeJunctionId left, GradeJunctionId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(GradeJunctionId left, GradeJunctionId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(GradeJunctionId left, GradeJunctionId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(GradeJunctionId left, GradeJunctionId right)
    {
        return left.CompareTo(right) >= 0;
    }
}
