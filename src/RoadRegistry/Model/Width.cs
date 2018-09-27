namespace RoadRegistry.Model
{
    using System;

    public readonly struct Width : IEquatable<Width>
    {
        private readonly int _value;

        public Width(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The width must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(Width other) => _value == other._value;
        public override bool Equals(object other) => other is Width revision && Equals(revision);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static bool operator ==(Width left, Width right) => left.Equals(right);
        public static bool operator !=(Width left, Width right) => !left.Equals(right);
        public static implicit operator int(Width instance) => instance._value;
    }
}
