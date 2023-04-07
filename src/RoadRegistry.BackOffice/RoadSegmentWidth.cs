namespace RoadRegistry.BackOffice;

using System;
using System.Diagnostics.Contracts;

public readonly struct RoadSegmentWidth : IEquatable<RoadSegmentWidth>
{
    private const int MaximumValue = 50;
    private const int UnknownValue = -8;
    private const int NotApplicableValue = -9;
    public static readonly RoadSegmentWidth Unknown = new(UnknownValue);
    public static readonly RoadSegmentWidth NotApplicable = new(NotApplicableValue);
    public static readonly RoadSegmentWidth Maximum = new(MaximumValue);
    private readonly int _value;

    public RoadSegmentWidth(int value)
    {
        if (!Accepts(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The road segment width must be greater than or equal to 0 and less than or equal to {MaximumValue}, or {UnknownValue} (unknown) or {NotApplicableValue} (not applicable).");
        }

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value == UnknownValue || value == NotApplicableValue || (0 <= value && value <= MaximumValue);
    }

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(RoadSegmentWidth other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentWidth revision && Equals(revision);
    }

    public override int GetHashCode()
    {
        return _value;
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public static bool operator ==(RoadSegmentWidth left, RoadSegmentWidth right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentWidth left, RoadSegmentWidth right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadSegmentWidth instance)
    {
        return instance._value;
    }
}
