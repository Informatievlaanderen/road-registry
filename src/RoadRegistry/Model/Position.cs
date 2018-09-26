namespace RoadRegistry.Model
{
    using System;

    public readonly struct Position : IEquatable<Position>, IComparable<Position>
    {
        private readonly double _value;

        public Position(double value)
        {
            if (value < 0.0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The position must be greater than or equal to zero.");

            _value = value;
        }

        public double ToDouble() => _value;
        public bool Equals(Position other) => Math.Abs(_value - other._value) < double.Epsilon;
        public override bool Equals(object other) => other is Position id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString();
        public int CompareTo(Position other) => _value.CompareTo(other._value);
        public static bool operator ==(Position left, Position right) => left.Equals(right);
        public static bool operator !=(Position left, Position right) => !left.Equals(right);
        public static bool operator <(Position left, Position right) => left.CompareTo(right) == -1;
        public static bool operator <=(Position left, Position right) => left.CompareTo(right) <= 0;
        public static bool operator >(Position left, Position right) => left.CompareTo(right) == 1;
        public static bool operator >=(Position left, Position right) => left.CompareTo(right) >= 0;
        public static implicit operator Double(Position instance) => instance._value;
    }
}
