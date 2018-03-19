namespace RoadRegistry
{
    using System;

    public readonly struct RoadNodeId : IEquatable<RoadNodeId>
    {
        private readonly long _value;

        public RoadNodeId(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road node identifier must be greater than or equal to zero.");
            }
            _value = value;
        }

        public bool Equals(RoadNodeId other) => _value == other._value;
        public override bool Equals(object other) => other is RoadNodeId && Equals((RoadNodeId)other);
        public override int GetHashCode() => _value.GetHashCode();
    }
}
