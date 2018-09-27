namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentDirection : IEquatable<RoadSegmentDirection>
    {
        private readonly int _value;

        public static readonly RoadSegmentDirection Unknown = new RoadSegmentDirection(-8);
        public static readonly RoadSegmentDirection Forward = new RoadSegmentDirection(1);
        public static readonly RoadSegmentDirection Backward = new RoadSegmentDirection(2);

        public static readonly RoadSegmentDirection[] All = {
            Unknown, Forward, Backward
        };

        private RoadSegmentDirection(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentDirection parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentDirection Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known numbered road segment direction.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentDirection other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentDirection type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(RoadSegmentDirection instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentDirection left, RoadSegmentDirection right) => Equals(left, right);
        public static bool operator !=(RoadSegmentDirection left, RoadSegmentDirection right) => !Equals(left, right);
    }
}
