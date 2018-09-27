namespace RoadRegistry.Model
{
    using System;

    public readonly struct RoadSegmentLaneCount : IEquatable<RoadSegmentLaneCount>
    {
        private readonly int _value;

        public RoadSegmentLaneCount(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment lane count must be greater than or equal to zero.");

            _value = value;
        }

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
