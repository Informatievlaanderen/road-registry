namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentHardeningType : IEquatable<RoadSegmentHardeningType>
    {
        private readonly int _value;

        public static readonly RoadSegmentHardeningType NotApplicable = new RoadSegmentHardeningType(-9);
        public static readonly RoadSegmentHardeningType Unknown = new RoadSegmentHardeningType(-8);
        public static readonly RoadSegmentHardeningType SolidHardening = new RoadSegmentHardeningType(1);
        public static readonly RoadSegmentHardeningType LooseHardening = new RoadSegmentHardeningType(2);

        public static readonly RoadSegmentHardeningType[] All = {
            NotApplicable, Unknown, SolidHardening, LooseHardening
        };

        private RoadSegmentHardeningType(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentHardeningType parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentHardeningType Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road hardening.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentHardeningType other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentHardeningType type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(RoadSegmentHardeningType instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentHardeningType left, RoadSegmentHardeningType right) => Equals(left, right);
        public static bool operator !=(RoadSegmentHardeningType left, RoadSegmentHardeningType right) => !Equals(left, right);
    }
}
