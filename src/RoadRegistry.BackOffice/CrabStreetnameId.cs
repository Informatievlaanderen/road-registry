namespace RoadRegistry.BackOffice
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    public readonly struct CrabStreetnameId : IEquatable<CrabStreetnameId>
    {
        private const int UnknownValue = -8;
        private const int NotApplicableValue = -9;
        public static readonly CrabStreetnameId Unknown = new CrabStreetnameId(UnknownValue);
        public static readonly CrabStreetnameId NotApplicable = new CrabStreetnameId(NotApplicableValue);
        private readonly int _value;

        public CrabStreetnameId(int value)
        {
            if (value != UnknownValue
                && value != NotApplicableValue
                && value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The crab street name identifier must be greater than or equal to zero.");
            }

            _value = value;
        }

        public static bool Accepts(int value)
        {
            return value == UnknownValue || value == NotApplicableValue || 0 <= value;
        }

        [Pure]
        public int ToInt32() => _value;
        public bool Equals(CrabStreetnameId other) => _value == other._value;
        public override bool Equals(object other) => other is CrabStreetnameId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
        public static bool operator ==(CrabStreetnameId left, CrabStreetnameId right) => left.Equals(right);
        public static bool operator !=(CrabStreetnameId left, CrabStreetnameId right) => !left.Equals(right);
        public static implicit operator int(CrabStreetnameId instance) => instance._value;
    }
}
