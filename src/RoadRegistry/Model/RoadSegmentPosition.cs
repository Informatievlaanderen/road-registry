namespace RoadRegistry.Model
{
    using System;

    public readonly struct RoadSegmentPosition : IEquatable<RoadSegmentPosition>, IComparable<RoadSegmentPosition>
    {
        private readonly double _value;

        public RoadSegmentPosition(double value)
        {
            if (value < 0.0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment position must be greater than or equal to zero.");

            _value = value;
        }

        public double ToDouble() => _value;
        public bool Equals(RoadSegmentPosition other) => Math.Abs(_value - other._value) < double.Epsilon;
        public override bool Equals(object other) => other is RoadSegmentPosition id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString();
        public int CompareTo(RoadSegmentPosition other) => _value.CompareTo(other._value);
        public static bool operator ==(RoadSegmentPosition left, RoadSegmentPosition right) => left.Equals(right);
        public static bool operator !=(RoadSegmentPosition left, RoadSegmentPosition right) => !left.Equals(right);
        public static bool operator <(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) == -1;
        public static bool operator <=(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) <= 0;
        public static bool operator >(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) == 1;
        public static bool operator >=(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) >= 0;
        public static implicit operator Double(RoadSegmentPosition instance) => instance._value;
    }
}
