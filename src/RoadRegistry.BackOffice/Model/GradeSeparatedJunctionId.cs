namespace RoadRegistry.BackOffice.Model
{
    using System;

    public readonly struct GradeSeparatedJunctionId : IEquatable<GradeSeparatedJunctionId>, IComparable<GradeSeparatedJunctionId>
    {
        private readonly int _value;

        public GradeSeparatedJunctionId(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The grade separated junction identifier must be greater than or equal to zero.");

            _value = value;
        }

        public static bool Accepts(int value)
        {
            return value >= 0;
        }

        public GradeSeparatedJunctionId Next()
        {
            if (_value == int.MaxValue)
            {
                throw new NotSupportedException(
                    "There is no next grade separated junction identifier because the maximum of the integer data type has been reached.");
            }
            return new GradeSeparatedJunctionId(_value + 1);
        }

        public static GradeSeparatedJunctionId Max(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right) =>
            new GradeSeparatedJunctionId(Math.Max(left._value, right._value));
        public static GradeSeparatedJunctionId Min(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right) =>
            new GradeSeparatedJunctionId(Math.Min(left._value, right._value));

        public int ToInt32() => _value;
        public bool Equals(GradeSeparatedJunctionId other) => _value == other._value;
        public override bool Equals(object other) => other is GradeSeparatedJunctionId id && Equals(id);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => "GSJ-" + _value;
        public int CompareTo(GradeSeparatedJunctionId other) => _value.CompareTo(other._value);
        public static bool operator ==(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right) => left.Equals(right);
        public static bool operator !=(GradeSeparatedJunctionId left, GradeSeparatedJunctionId right) => !left.Equals(right);
        public static implicit operator int(GradeSeparatedJunctionId instance) => instance._value;
    }
}
