namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    public readonly struct RoadSegmentPosition : IEquatable<RoadSegmentPosition>, IComparable<RoadSegmentPosition>
    {
        public static readonly RoadSegmentPosition Zero = new RoadSegmentPosition(0.0m);

        private readonly decimal _value;

        public RoadSegmentPosition(decimal value)
        {
            if (value < 0.0m)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment position must be greater than or equal to zero.");

            _value = value;
        }

        public static RoadSegmentPosition FromDouble(double value)
        {
            return new RoadSegmentPosition(Convert.ToDecimal(value));
        }

        [Pure]
        public decimal ToDecimal() => _value;
        [Pure]
        public double ToDouble() => decimal.ToDouble(_value);
        public bool Equals(RoadSegmentPosition other) => _value.Equals(other._value);
        public override bool Equals(object other) => other is RoadSegmentPosition id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
        public int CompareTo(RoadSegmentPosition other) => _value.CompareTo(other._value);
        public static bool operator ==(RoadSegmentPosition left, RoadSegmentPosition right) => left.Equals(right);
        public static bool operator !=(RoadSegmentPosition left, RoadSegmentPosition right) => !left.Equals(right);
        public static bool operator <(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) == -1;
        public static bool operator <=(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) <= 0;
        public static bool operator >(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) == 1;
        public static bool operator >=(RoadSegmentPosition left, RoadSegmentPosition right) => left.CompareTo(right) >= 0;
        public static implicit operator decimal(RoadSegmentPosition instance) => instance._value;
    }
}
