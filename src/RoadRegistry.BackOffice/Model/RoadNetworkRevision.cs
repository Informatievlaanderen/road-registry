namespace RoadRegistry.BackOffice.Model
{
    using System;

    public readonly struct RoadNetworkRevision : IEquatable<RoadNetworkRevision>
    {
        private readonly int _value;

        public RoadNetworkRevision(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road revision must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(RoadNetworkRevision other) => _value == other._value;
        public override bool Equals(object other) => other is RoadNetworkRevision revision && Equals(revision);
        public override int GetHashCode() => _value.GetHashCode();
        public static bool operator ==(RoadNetworkRevision left, RoadNetworkRevision right) => left.Equals(right);
        public static bool operator !=(RoadNetworkRevision left, RoadNetworkRevision right) => !left.Equals(right);
        public static implicit operator int(RoadNetworkRevision instance) => instance._value;
    }
}
