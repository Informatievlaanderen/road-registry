namespace RoadRegistry
{
    using System;

    public readonly struct RoadNetworkRevision : IEquatable<RoadNetworkRevision>
    {
        private readonly long _value;

        public RoadNetworkRevision(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road revision must be greater than or equal to zero.");
            }
            _value = value;
        }
        public long ToInt64() => _value;
        public bool Equals(RoadNetworkRevision other) => _value == other._value;
        public override bool Equals(object other) => other is RoadNetworkRevision && Equals((RoadNetworkRevision)other);
        public override int GetHashCode() => _value.GetHashCode();
        public static bool operator ==(RoadNetworkRevision left, RoadNetworkRevision right) => left.Equals(right);
        public static bool operator !=(RoadNetworkRevision left, RoadNetworkRevision right) => !left.Equals(right);
        public static implicit operator Int64(RoadNetworkRevision instance) => instance._value;
    }
}
