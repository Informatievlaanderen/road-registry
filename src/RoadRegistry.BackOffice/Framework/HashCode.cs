namespace RoadRegistry.BackOffice.Framework
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    // original source: https://github.com/nodatime/nodatime/blob/master/src/NodaTime/Utility/HashCodeHelper.cs
    internal readonly struct HashCode : IEquatable<HashCode>
    {
        public static readonly HashCode Initial = new HashCode(HashCodeInitializer);

        public static HashCode FromHashCode(int value) => new HashCode(value);

        private const int HashCodeMultiplier = 37;
        private const int HashCodeInitializer = 17;

        private readonly int _value;

        private HashCode(int value)
        {
            _value = value;
        }

        [Pure]
        public HashCode Hash<T>(T value)
        {
            unchecked
            {
                return new HashCode(_value * HashCodeMultiplier + (value?.GetHashCode() ?? 0));
            }
        }

        [Pure]
        public bool Equals(HashCode other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is HashCode other && Equals(other);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
        public static implicit operator int(HashCode instance) => instance._value;
    }
}
