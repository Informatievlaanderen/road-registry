namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentGeometryDrawMethod : IEquatable<RoadSegmentGeometryDrawMethod>
    {
        public static readonly RoadSegmentGeometryDrawMethod Outlined = 
            new RoadSegmentGeometryDrawMethod(1);
        public static readonly RoadSegmentGeometryDrawMethod Measured = 
            new RoadSegmentGeometryDrawMethod(2);
        public static readonly RoadSegmentGeometryDrawMethod Measured_according_to_GRB_specifications = 
            new RoadSegmentGeometryDrawMethod(3);

        public static readonly RoadSegmentGeometryDrawMethod[] All = {
            Outlined, Measured, Measured_according_to_GRB_specifications
        };

        private readonly int _value;
        private RoadSegmentGeometryDrawMethod(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentGeometryDrawMethod parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentGeometryDrawMethod Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road segment geometry draw method.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentGeometryDrawMethod other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentGeometryDrawMethod type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;
        public static implicit operator int(RoadSegmentGeometryDrawMethod instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentGeometryDrawMethod left, RoadSegmentGeometryDrawMethod right) => Equals(left, right);
        public static bool operator !=(RoadSegmentGeometryDrawMethod left, RoadSegmentGeometryDrawMethod right) => !Equals(left, right);
    }
}