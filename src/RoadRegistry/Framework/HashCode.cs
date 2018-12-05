namespace RoadRegistry.Framework
{
    using System;
    using System.Diagnostics.Contracts;

    // original source: https://github.com/nodatime/nodatime/blob/master/src/NodaTime/Utility/HashCodeHelper.cs
    internal readonly struct HashCode : IEquatable<HashCode>
    {
        public static readonly HashCode Initial = new HashCode(HashCodeInitializer);

        private const int HashCodeMultiplier = 37;
        private const int HashCodeInitializer = 17;

        private readonly int _value;

        private HashCode(int value)
        {
            _value = value;
        }

        [Pure]
        public HashCode CombineWith<T>(T value)
        {
            unchecked
            {
                return new HashCode(_value * HashCodeMultiplier + (value?.GetHashCode() ?? 0));
            }
        }

        public static HashCode Hash<T>(T value)
        {
            unchecked
            {
                return new HashCode(HashCodeInitializer * HashCodeMultiplier + (value?.GetHashCode() ?? 0));
            }
        }

        public static int Hash<T1, T2>(T1 t1, T2 t2)
        {
            unchecked
            {
                var hash = HashCodeInitializer;
                hash = hash * HashCodeMultiplier + (t1?.GetHashCode() ?? 0);
                hash = hash * HashCodeMultiplier + (t2?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static int Hash<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
        {
            unchecked
            {
                var hash = HashCodeInitializer;
                hash = hash * HashCodeMultiplier + (t1?.GetHashCode() ?? 0);
                hash = hash * HashCodeMultiplier + (t2?.GetHashCode() ?? 0);
                hash = hash * HashCodeMultiplier + (t3?.GetHashCode() ?? 0);
                return hash;
            }
        }

        [Pure]
        public bool Equals(HashCode other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is HashCode other && Equals(other);
        public override int GetHashCode() => _value;
        public static implicit operator int(HashCode instance) => instance._value;
    }
}
