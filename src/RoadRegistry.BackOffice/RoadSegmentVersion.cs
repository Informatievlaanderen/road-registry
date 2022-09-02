namespace RoadRegistry.BackOffice
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    public readonly struct RoadSegmentVersion : IEquatable<RoadSegmentVersion>, IComparable<RoadSegmentVersion>
    {
        private readonly int _value;

        public RoadSegmentVersion(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment version must be greater than or equal to zero.");
            }

            _value = value;
        }

        public static bool Accepts(int value)
        {
            return value >= 0;
        }

        public RoadSegmentVersion Next()
        {
            if (_value == int.MaxValue)
            {
                throw new NotSupportedException(
                    "There is no next road segment version because the maximum of the integer data type has been reached.");
            }
            return new RoadSegmentVersion(_value + 1);
        }

        public static RoadSegmentVersion Max(RoadSegmentVersion left, RoadSegmentVersion right) =>
            new RoadSegmentVersion(Math.Max(left._value, right._value));
        public static RoadSegmentVersion Min(RoadSegmentVersion left, RoadSegmentVersion right) =>
            new RoadSegmentVersion(Math.Min(left._value, right._value));

        [Pure]
        public int ToInt32() => _value;
        
        public bool Equals(RoadSegmentVersion other) => _value == other._value;
        
        public override bool Equals(object obj) => obj is RoadSegmentVersion id && Equals(id);
        
        public override int GetHashCode() => _value.GetHashCode();
        
        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
        
        public int CompareTo(RoadSegmentVersion other) => _value.CompareTo(other._value);
        
        public static bool operator ==(RoadSegmentVersion left, RoadSegmentVersion right) => left.Equals(right);
        
        public static bool operator !=(RoadSegmentVersion left, RoadSegmentVersion right) => !left.Equals(right);
        
        public static implicit operator int(RoadSegmentVersion instance) => instance._value;

        public static bool operator <(RoadSegmentVersion left, RoadSegmentVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(RoadSegmentVersion left, RoadSegmentVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(RoadSegmentVersion left, RoadSegmentVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(RoadSegmentVersion left, RoadSegmentVersion right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
