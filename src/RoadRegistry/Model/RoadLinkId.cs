namespace RoadRegistry.Model
{
    using System;

    public readonly struct RoadLinkId : IEquatable<RoadLinkId>, IComparable<RoadLinkId>
    {
        private readonly long _value;

        public RoadLinkId(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road link identifier must be greater than or equal to zero.");
            }
            _value = value;
        }

        public long ToInt64() => _value;
        public bool Equals(RoadLinkId other) => _value == other._value;
        public override bool Equals(object other) => other is RoadLinkId && Equals((RoadLinkId)other);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => "RS-" + _value.ToString();
        public int CompareTo(RoadLinkId other) => _value.CompareTo(other._value);
        public static bool operator ==(RoadLinkId left, RoadLinkId right) => left.Equals(right);
        public static bool operator !=(RoadLinkId left, RoadLinkId right) => !left.Equals(right);
        public static implicit operator Int64(RoadLinkId instance) => instance._value;
    }
}
