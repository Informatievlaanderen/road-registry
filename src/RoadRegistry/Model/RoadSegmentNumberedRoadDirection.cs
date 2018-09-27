namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentNumberedRoadDirection : IEquatable<RoadSegmentNumberedRoadDirection>
    {
        private readonly int _value;

        public static readonly RoadSegmentNumberedRoadDirection Unknown = new RoadSegmentNumberedRoadDirection(-8);
        public static readonly RoadSegmentNumberedRoadDirection Forward = new RoadSegmentNumberedRoadDirection(1);
        public static readonly RoadSegmentNumberedRoadDirection Backward = new RoadSegmentNumberedRoadDirection(2);

        public static readonly RoadSegmentNumberedRoadDirection[] All = {
            Unknown, Forward, Backward
        };

        private RoadSegmentNumberedRoadDirection(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentNumberedRoadDirection parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentNumberedRoadDirection Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known numbered road segment direction.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentNumberedRoadDirection other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentNumberedRoadDirection type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(RoadSegmentNumberedRoadDirection instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right) => Equals(left, right);
        public static bool operator !=(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right) => !Equals(left, right);
    }
}
