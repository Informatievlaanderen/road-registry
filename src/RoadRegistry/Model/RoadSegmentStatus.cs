namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentStatus : IEquatable<RoadSegmentStatus>
    {
        public static readonly RoadSegmentStatus Unknown = new RoadSegmentStatus(-8);
        public static readonly RoadSegmentStatus PermitRequested = new RoadSegmentStatus(1);
        public static readonly RoadSegmentStatus BuildingPermitGranted = new RoadSegmentStatus(2);
        public static readonly RoadSegmentStatus UnderConstruction = new RoadSegmentStatus(3);
        public static readonly RoadSegmentStatus InUse = new RoadSegmentStatus(4);
        public static readonly RoadSegmentStatus OutOfUse = new RoadSegmentStatus(5);

        public static readonly RoadSegmentStatus[] All = {
            Unknown, PermitRequested, BuildingPermitGranted, UnderConstruction, InUse, OutOfUse
        };
        
        private readonly int _value;

        private RoadSegmentStatus(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentStatus parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentStatus Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road segment status.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentStatus other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentStatus type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;
        public static implicit operator int(RoadSegmentStatus instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentStatus left, RoadSegmentStatus right) => Equals(left, right);
        public static bool operator !=(RoadSegmentStatus left, RoadSegmentStatus right) => !Equals(left, right);
    }
}