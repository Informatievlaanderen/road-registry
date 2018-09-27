namespace RoadRegistry.Model
{
    using System;

    public readonly struct RoadSegmentWidth : IEquatable<RoadSegmentWidth>
    {
        private readonly int _value;

        public RoadSegmentWidth(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment width must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(RoadSegmentWidth other) => _value == other._value;
        public override bool Equals(object other) => other is RoadSegmentWidth revision && Equals(revision);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static bool operator ==(RoadSegmentWidth left, RoadSegmentWidth right) => left.Equals(right);
        public static bool operator !=(RoadSegmentWidth left, RoadSegmentWidth right) => !left.Equals(right);
        public static implicit operator int(RoadSegmentWidth instance) => instance._value;
    }
}
