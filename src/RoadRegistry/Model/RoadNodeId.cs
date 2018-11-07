namespace RoadRegistry.Model
{
    using System;

    public readonly struct RoadNodeId : IEquatable<RoadNodeId>, IComparable<RoadNodeId>
    {
        private readonly int _value;

        public RoadNodeId(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road node identifier must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(RoadNodeId other) => _value == other._value;
        public override bool Equals(object other) => other is RoadNodeId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => "RN-" + _value;
        public int CompareTo(RoadNodeId other) => _value.CompareTo(other._value);
        public static bool operator ==(RoadNodeId left, RoadNodeId right) => left.Equals(right);
        public static bool operator !=(RoadNodeId left, RoadNodeId right) => !left.Equals(right);
        public static implicit operator int(RoadNodeId instance) => instance._value;
    }
}
