namespace RoadRegistry.BackOffice
{
    using System;

    public readonly struct GeometryVersion : IEquatable<GeometryVersion>, IComparable<GeometryVersion>
    {
        private readonly int _value;

        public GeometryVersion(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The geometry version must be greater than or equal to zero.");
            }

            _value = value;
        }

        public int ToInt32() => _value;
        public GeometryVersion Next()
        {
            if (_value == int.MaxValue)
            {
                throw new NotSupportedException(
                    "There is no next geometry version because the maximum of the integer data type has been reached.");
            }
            return new GeometryVersion(_value + 1);
        }

        public bool Equals(GeometryVersion other) => _value == other._value;
        public override bool Equals(object obj) => obj is GeometryVersion version && Equals(version);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString();

        public int CompareTo(GeometryVersion other) => _value.CompareTo(other._value);

        public static bool operator ==(GeometryVersion left, GeometryVersion right) => left.Equals(right);
        public static bool operator !=(GeometryVersion left, GeometryVersion right) => !left.Equals(right);

        public static bool operator <(GeometryVersion left, GeometryVersion right) => left.CompareTo(right) == -1;
        public static bool operator <=(GeometryVersion left, GeometryVersion right) => left.CompareTo(right) <= 0;
        public static bool operator >(GeometryVersion left, GeometryVersion right) => left.CompareTo(right) == 1;
        public static bool operator >=(GeometryVersion left, GeometryVersion right) => left.CompareTo(right) >= 0;

        public static implicit operator int(GeometryVersion instance) => instance._value;
    }
}
