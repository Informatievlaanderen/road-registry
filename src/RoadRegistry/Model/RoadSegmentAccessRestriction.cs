namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentAccessRestriction : IEquatable<RoadSegmentAccessRestriction>
    {
        public static readonly RoadSegmentAccessRestriction PublicRoad = new RoadSegmentAccessRestriction(1);
        public static readonly RoadSegmentAccessRestriction PhysicallyImpossible = new RoadSegmentAccessRestriction(2);
        public static readonly RoadSegmentAccessRestriction LegallyForbidden = new RoadSegmentAccessRestriction(3);
        public static readonly RoadSegmentAccessRestriction PrivateRoad = new RoadSegmentAccessRestriction(4);
        public static readonly RoadSegmentAccessRestriction Seasonal = new RoadSegmentAccessRestriction(5);
        public static readonly RoadSegmentAccessRestriction Toll = new RoadSegmentAccessRestriction(6);

        public static readonly RoadSegmentAccessRestriction[] All = {
            PublicRoad, PhysicallyImpossible, LegallyForbidden,
            PrivateRoad, Seasonal, Toll
        };

        private readonly int _value;

        private RoadSegmentAccessRestriction(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentAccessRestriction parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentAccessRestriction Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road segment access restriction.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentAccessRestriction other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentAccessRestriction type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;
        public static implicit operator int(RoadSegmentAccessRestriction instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentAccessRestriction left, RoadSegmentAccessRestriction right) => Equals(left, right);
        public static bool operator !=(RoadSegmentAccessRestriction left, RoadSegmentAccessRestriction right) => !Equals(left, right);
    }
}