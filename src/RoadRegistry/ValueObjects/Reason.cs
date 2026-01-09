namespace RoadRegistry.ValueObjects;

using System;

public readonly struct Reason : IEquatable<Reason>
{
    public static readonly Reason None = default;
    public const int MaxLength = 254;
    private readonly string _value;

    public Reason(string value)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value), "The reason must not be null or empty.");

        if (value.Length > MaxLength)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"The reason must be {MaxLength} characters or less.");

        _value = value;
    }

    public bool Equals(Reason other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is Reason reason && Equals(reason);
    }

    public override int GetHashCode()
    {
        return _value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string?(Reason? instance)
    {
        return instance?.ToString();
    }

    public static bool operator ==(Reason left, Reason right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Reason left, Reason right)
    {
        return !left.Equals(right);
    }
}
