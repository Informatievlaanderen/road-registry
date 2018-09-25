namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentCategory : IEquatable<RoadSegmentCategory>
    {
        public static readonly RoadSegmentCategory Unknown = new RoadSegmentCategory("-8");
        public static readonly RoadSegmentCategory NotApplicable = new RoadSegmentCategory("-9");
        public static readonly RoadSegmentCategory MainRoad = new RoadSegmentCategory("H");
        public static readonly RoadSegmentCategory LocalRoad = new RoadSegmentCategory("L");
        public static readonly RoadSegmentCategory LocalRoadType1 = new RoadSegmentCategory("L1");
        public static readonly RoadSegmentCategory LocalRoadType2 = new RoadSegmentCategory("L2");
        public static readonly RoadSegmentCategory LocalRoadType3 = new RoadSegmentCategory("L3");
        public static readonly RoadSegmentCategory PrimaryRoadI = new RoadSegmentCategory("PI");
        public static readonly RoadSegmentCategory PrimaryRoadII = new RoadSegmentCategory("PII");
        public static readonly RoadSegmentCategory PrimaryRoadIIType1 = new RoadSegmentCategory("PII-1");
        public static readonly RoadSegmentCategory PrimaryRoadIIType2 = new RoadSegmentCategory("PII-2");
        public static readonly RoadSegmentCategory PrimaryRoadIIType3 = new RoadSegmentCategory("PII-3");
        public static readonly RoadSegmentCategory PrimaryRoadIIType4 = new RoadSegmentCategory("PII-4");
        public static readonly RoadSegmentCategory SecondaryRoad = new RoadSegmentCategory("S");
        public static readonly RoadSegmentCategory SecondaryRoadType1 = new RoadSegmentCategory("S1");
        public static readonly RoadSegmentCategory SecondaryRoadType2 = new RoadSegmentCategory("S2");
        public static readonly RoadSegmentCategory SecondaryRoadType3 = new RoadSegmentCategory("S3");
        public static readonly RoadSegmentCategory SecondaryRoadType4 = new RoadSegmentCategory("S4");

        public static readonly RoadSegmentCategory[] All = {
            Unknown, 
            NotApplicable, 
            MainRoad, 
            LocalRoad, LocalRoadType1, LocalRoadType2, LocalRoadType3,
            PrimaryRoadI, PrimaryRoadII, PrimaryRoadIIType1, PrimaryRoadIIType2, PrimaryRoadIIType3, PrimaryRoadIIType4,
            SecondaryRoad, SecondaryRoadType1, SecondaryRoadType2, SecondaryRoadType3, SecondaryRoadType4
        };

        private readonly string _value;
        private RoadSegmentCategory(string value)
        {
            _value = value;
        }

        public static bool TryParse(string value, out RoadSegmentCategory parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentCategory Parse(string value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road segment category.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentCategory other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentCategory type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator String(RoadSegmentCategory instance) => instance.ToString();
        public static bool operator ==(RoadSegmentCategory left, RoadSegmentCategory right) => Equals(left, right);
        public static bool operator !=(RoadSegmentCategory left, RoadSegmentCategory right) => !Equals(left, right);
    }
}