namespace RoadRegistry.Model
{
    using System;

    public readonly struct RoadSegmentId : IEquatable<RoadSegmentId>, IComparable<RoadSegmentId>
    {
        private readonly int _value;

        public RoadSegmentId(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road link identifier must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(RoadSegmentId other) => _value == other._value;
        public override bool Equals(object other) => other is RoadSegmentId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => "RS-" + _value;
        public int CompareTo(RoadSegmentId other) => _value.CompareTo(other._value);
        public static bool operator ==(RoadSegmentId left, RoadSegmentId right) => left.Equals(right);
        public static bool operator !=(RoadSegmentId left, RoadSegmentId right) => !left.Equals(right);
        public static implicit operator int(RoadSegmentId instance) => instance._value;
    }
}
