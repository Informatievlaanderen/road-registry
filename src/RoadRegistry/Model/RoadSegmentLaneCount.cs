namespace RoadRegistry.Model
{
    using System;
    using System.Diagnostics.Contracts;

    public readonly struct RoadSegmentLaneCount : IEquatable<RoadSegmentLaneCount>
    {
        private const int MaximumValue = 7;
        private const int UnknownValue = -8;
        private const int NotApplicableValue = -9;
        public static readonly RoadSegmentLaneCount Maximum = new RoadSegmentLaneCount(MaximumValue);
        public static readonly RoadSegmentLaneCount Unknown = new RoadSegmentLaneCount(UnknownValue);
        public static readonly RoadSegmentLaneCount NotApplicable = new RoadSegmentLaneCount(NotApplicableValue);

        private readonly int _value;

        public RoadSegmentLaneCount(int value)
        {
            if (value != UnknownValue && value != NotApplicableValue)
            {
                if (value < 0 || value > MaximumValue)
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "The road segment lane count must be greater than or equal to 0 and less than or equal to 7.");
            }

            _value = value;
        }

        [Pure]
        public int ToInt32() => _value;
        public bool Equals(RoadSegmentLaneCount other) => _value == other._value;
        public override bool Equals(object other) => other is RoadSegmentLaneCount revision && Equals(revision);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static bool operator ==(RoadSegmentLaneCount left, RoadSegmentLaneCount right) => left.Equals(right);
        public static bool operator !=(RoadSegmentLaneCount left, RoadSegmentLaneCount right) => !left.Equals(right);
        public static implicit operator int(RoadSegmentLaneCount instance) => instance._value;
    }
}
