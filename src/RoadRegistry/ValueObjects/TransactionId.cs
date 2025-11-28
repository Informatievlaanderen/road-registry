namespace RoadRegistry.ValueObjects;

using System;
using System.Globalization;

public readonly struct TransactionId : IEquatable<TransactionId>, IComparable<TransactionId>
{
    private const int UnknownValue = -8;
    public static readonly TransactionId Unknown = new(UnknownValue);
    private readonly int _value;

    public TransactionId(int value)
    {
        if (value != UnknownValue && value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "The transaction identifier must be greater than or equal to zero, or -8 (unknown).");

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value == UnknownValue || value >= 0;
    }

    public TransactionId Next()
    {
        if (_value == int.MaxValue)
            throw new NotSupportedException(
                "There is no next transaction identifier because the maximum of the integer data type has been reached.");
        return new TransactionId(_value + 1);
    }

    public static TransactionId Max(TransactionId left, TransactionId right)
    {
        return new TransactionId(Math.Max(left._value, right._value));
    }

    public static TransactionId Min(TransactionId left, TransactionId right)
    {
        return new TransactionId(Math.Min(left._value, right._value));
    }

    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(TransactionId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is TransactionId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public int CompareTo(TransactionId other)
    {
        return _value.CompareTo(other._value);
    }

    public static bool operator ==(TransactionId left, TransactionId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TransactionId left, TransactionId right)
    {
        return !left.Equals(right);
    }

    public static bool operator <=(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator <(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >=(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator >(TransactionId left, TransactionId right)
    {
        return left.CompareTo(right) > 0;
    }

    public static implicit operator int(TransactionId instance)
    {
        return instance._value;
    }
}
