namespace RoadRegistry.BackOffice
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

        public static bool Accepts(int value)
        {
            return value >= 0;
        }

        public RoadSegmentId Next()
        {
            if (_value == int.MaxValue)
            {
                throw new NotSupportedException(
                    "There is no next road segment identifier because the maximum of the integer data type has been reached.");
            }
            return new RoadSegmentId(_value + 1);
        }

        public static RoadSegmentId Max(RoadSegmentId left, RoadSegmentId right) =>
            new RoadSegmentId(Math.Max(left._value, right._value));
        public static RoadSegmentId Min(RoadSegmentId left, RoadSegmentId right) =>
            new RoadSegmentId(Math.Min(left._value, right._value));

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
