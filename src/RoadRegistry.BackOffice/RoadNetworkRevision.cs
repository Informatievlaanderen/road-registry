namespace RoadRegistry.BackOffice
{
    using System;

    public readonly struct RoadNetworkRevision : IEquatable<RoadNetworkRevision>
    {
        private readonly int _value;

        public RoadNetworkRevision(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road revision must be greater than or equal to zero.");
            }

            _value = value;
        }

        public int ToInt32() => _value;
        public RoadNetworkRevision Next()
        {
            if (_value == int.MaxValue)
            {
                throw new NotSupportedException(
                    "There is no next road network revision because the maximum of the integer data type has been reached.");
            }
            return new RoadNetworkRevision(_value + 1);
        }

        public bool Equals(RoadNetworkRevision other) => _value == other._value;
        public override bool Equals(object obj) => obj is RoadNetworkRevision revision && Equals(revision);
        public override int GetHashCode() => _value.GetHashCode();
        public static bool operator ==(RoadNetworkRevision left, RoadNetworkRevision right) => left.Equals(right);
        public static bool operator !=(RoadNetworkRevision left, RoadNetworkRevision right) => !left.Equals(right);
        public static implicit operator int(RoadNetworkRevision instance) => instance._value;
    }
}
