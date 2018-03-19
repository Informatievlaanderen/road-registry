namespace RoadRegistry
{
    using System;

    public readonly struct RoadSegmentId : IEquatable<RoadSegmentId>
    {
        private readonly long _value;

        public RoadSegmentId(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment identifier must be greater than or equal to zero.");
            }
            _value = value;
        }

        public bool Equals(RoadSegmentId other) => _value == other._value;
        public override bool Equals(object other) => other is RoadSegmentId && Equals((RoadSegmentId)other);
        public override int GetHashCode() => _value.GetHashCode();
    }
}
