namespace RoadRegistry.Model
{
    using System;

    public class NumberedRoadSegmentDirection : IEquatable<NumberedRoadSegmentDirection>
    {
        private readonly int _value;

        public static readonly NumberedRoadSegmentDirection Unknown = new NumberedRoadSegmentDirection(-8);
        public static readonly NumberedRoadSegmentDirection Forward = new NumberedRoadSegmentDirection(1);
        public static readonly NumberedRoadSegmentDirection Backward = new NumberedRoadSegmentDirection(2);

        public static readonly NumberedRoadSegmentDirection[] All = {
            Unknown, Forward, Backward
        };

        private NumberedRoadSegmentDirection(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out NumberedRoadSegmentDirection parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static NumberedRoadSegmentDirection Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known numbered road segment direction.");
            }
            return parsed;
        }

        public bool Equals(NumberedRoadSegmentDirection other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is NumberedRoadSegmentDirection type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(NumberedRoadSegmentDirection instance) => instance.ToInt32();
        public static bool operator ==(NumberedRoadSegmentDirection left, NumberedRoadSegmentDirection right) => Equals(left, right);
        public static bool operator !=(NumberedRoadSegmentDirection left, NumberedRoadSegmentDirection right) => !Equals(left, right);
    }
}
