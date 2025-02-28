namespace RoadRegistry.BackOffice;

using System;

public readonly struct OperatorName : IEquatable<OperatorName>
{
    public const int MaxLength = 254;
    private readonly string _value;

    public static readonly OperatorName Unknown = new OperatorName("-8");

    public OperatorName(string value)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value), "The operator name must not be null or empty.");

        if (value.Length > MaxLength)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"The operator name must be {MaxLength} characters or less.");

        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length <= MaxLength;
    }

    public bool Equals(OperatorName other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is OperatorName name && Equals(name);
    }

    public override int GetHashCode()
    {
        return _value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(OperatorName instance)
    {
        return instance._value;
    }

    public static bool operator ==(OperatorName left, OperatorName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OperatorName left, OperatorName right)
    {
        return !left.Equals(right);
    }
}
