namespace RoadRegistry.Model
{
    using System;

    public readonly struct CrabStreetnameId : IEquatable<CrabStreetnameId>
    {
        private readonly int _value;

        public CrabStreetnameId(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The crab street name identifier must be greater than or equal to zero.");

            _value = value;
        }

        public int ToInt32() => _value;
        public bool Equals(CrabStreetnameId other) => _value == other._value;
        public override bool Equals(object other) => other is CrabStreetnameId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString();
        public static bool operator ==(CrabStreetnameId left, CrabStreetnameId right) => left.Equals(right);
        public static bool operator !=(CrabStreetnameId left, CrabStreetnameId right) => !left.Equals(right);
        public static implicit operator Int32(CrabStreetnameId instance) => instance._value;
    }
}
