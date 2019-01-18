namespace RoadRegistry.BackOffice.Model
{
    using System;

    public readonly struct RoadSegmentNumberedRoadOrdinal : IEquatable<RoadSegmentNumberedRoadOrdinal>
    {
        private readonly int _value;

        public RoadSegmentNumberedRoadOrdinal(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment numbered road ordinal must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(RoadSegmentNumberedRoadOrdinal other) => _value == other._value;
        public override bool Equals(object other) => other is RoadSegmentNumberedRoadOrdinal revision && Equals(revision);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static bool operator ==(RoadSegmentNumberedRoadOrdinal left, RoadSegmentNumberedRoadOrdinal right) => left.Equals(right);
        public static bool operator !=(RoadSegmentNumberedRoadOrdinal left, RoadSegmentNumberedRoadOrdinal right) => !left.Equals(right);
        public static implicit operator int(RoadSegmentNumberedRoadOrdinal instance) => instance._value;
    }
}
