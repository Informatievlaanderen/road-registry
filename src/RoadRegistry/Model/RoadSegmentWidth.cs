namespace RoadRegistry.Model
{
    using System;
    using System.Diagnostics.Contracts;

    public readonly struct RoadSegmentWidth : IEquatable<RoadSegmentWidth>
    {
        private const int MaximumValue = 45;

        public static readonly RoadSegmentWidth Unknown = new RoadSegmentWidth(-8);
        public static readonly RoadSegmentWidth NotApplicable = new RoadSegmentWidth(-9);
        public static readonly RoadSegmentWidth Maximum = new RoadSegmentWidth(MaximumValue);

        private readonly int _value;

        public RoadSegmentWidth(int value)
        {
            if ((value < 0 || value > MaximumValue) && value != -8 && value != -9)
                throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment width must be greater than or equal to 0 and less than or equal to 45, or -8 (unknown) or -9 (not applicable).");

            _value = value;
        }

        [Pure]
        public int ToInt32() => _value;
        public bool Equals(RoadSegmentWidth other) => _value == other._value;
        public override bool Equals(object other) => other is RoadSegmentWidth revision && Equals(revision);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public static bool operator ==(RoadSegmentWidth left, RoadSegmentWidth right) => left.Equals(right);
        public static bool operator !=(RoadSegmentWidth left, RoadSegmentWidth right) => !left.Equals(right);
        public static implicit operator int(RoadSegmentWidth instance) => instance._value;
    }
}
