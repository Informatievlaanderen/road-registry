namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentSurfaceType : IEquatable<RoadSegmentSurfaceType>
    {
        private readonly int _value;

        public static readonly RoadSegmentSurfaceType NotApplicable = new RoadSegmentSurfaceType(-9);
        public static readonly RoadSegmentSurfaceType Unknown = new RoadSegmentSurfaceType(-8);
        public static readonly RoadSegmentSurfaceType SolidSurface = new RoadSegmentSurfaceType(1);
        public static readonly RoadSegmentSurfaceType LooseSurface = new RoadSegmentSurfaceType(2);

        public static readonly RoadSegmentSurfaceType[] All = {
            NotApplicable, Unknown, SolidSurface, LooseSurface
        };

        private RoadSegmentSurfaceType(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentSurfaceType parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentSurfaceType Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road surface.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentSurfaceType other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentSurfaceType type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;

        public static implicit operator int(RoadSegmentSurfaceType instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentSurfaceType left, RoadSegmentSurfaceType right) => Equals(left, right);
        public static bool operator !=(RoadSegmentSurfaceType left, RoadSegmentSurfaceType right) => !Equals(left, right);
    }
}
