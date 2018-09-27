namespace RoadRegistry.Model
{
    using System;

    public readonly struct LaneCount : IEquatable<LaneCount>
    {
        private readonly int _value;

        public LaneCount(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The lane count must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(LaneCount other) => _value == other._value;
        public override bool Equals(object other) => other is LaneCount revision && Equals(revision);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static bool operator ==(LaneCount left, LaneCount right) => left.Equals(right);
        public static bool operator !=(LaneCount left, LaneCount right) => !left.Equals(right);
        public static implicit operator int(LaneCount instance) => instance._value;
    }
}
