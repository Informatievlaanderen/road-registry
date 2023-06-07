namespace RoadRegistry.BackOffice;

using System;
using System.Diagnostics.Contracts;

public readonly struct RoadSegmentLaneCount : IEquatable<RoadSegmentLaneCount>
{
    private const int MaximumValue = 10;
    private const int UnknownValue = -8;
    private const int NotApplicableValue = -9;
    public static readonly RoadSegmentLaneCount Maximum = new(MaximumValue);
    public static readonly RoadSegmentLaneCount Unknown = new(UnknownValue);
    public static readonly RoadSegmentLaneCount NotApplicable = new(NotApplicableValue);
    private readonly int _value;

    public RoadSegmentLaneCount(int value)
    {
        if (!Accepts(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The road segment lane count must be greater than or equal to 0 and less than or equal to {MaximumValue}.");
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

    public bool Equals(RoadSegmentLaneCount other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentLaneCount revision && Equals(revision);
    }

    public override int GetHashCode()
    {
        return _value;
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public static bool operator ==(RoadSegmentLaneCount left, RoadSegmentLaneCount right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentLaneCount left, RoadSegmentLaneCount right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadSegmentLaneCount instance)
    {
        return instance._value;
    }
}
