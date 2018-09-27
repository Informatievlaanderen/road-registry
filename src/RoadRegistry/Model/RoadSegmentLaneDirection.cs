namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentLaneDirection : IEquatable<RoadSegmentLaneDirection>
    {
        private readonly int _value;

        public static readonly RoadSegmentLaneDirection Unknown = new RoadSegmentLaneDirection(-8);
        public static readonly RoadSegmentLaneDirection Forward = new RoadSegmentLaneDirection(1);
        public static readonly RoadSegmentLaneDirection Backward = new RoadSegmentLaneDirection(2);
        public static readonly RoadSegmentLaneDirection Independent = new RoadSegmentLaneDirection(3);

        public static readonly RoadSegmentLaneDirection[] All = {
            Unknown, Forward, Backward, Independent
        };

        private RoadSegmentLaneDirection(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentLaneDirection parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentLaneDirection Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known lane direction.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentLaneDirection other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentLaneDirection type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(RoadSegmentLaneDirection instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentLaneDirection left, RoadSegmentLaneDirection right) => Equals(left, right);
        public static bool operator !=(RoadSegmentLaneDirection left, RoadSegmentLaneDirection right) => !Equals(left, right);
    }
}
