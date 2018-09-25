namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentMorphology : IEquatable<RoadSegmentMorphology>
    {
        public static readonly RoadSegmentMorphology Unknown = new RoadSegmentMorphology(-8);
        public static readonly RoadSegmentMorphology Motorway = new RoadSegmentMorphology(101);
        public static readonly RoadSegmentMorphology Road_with_separate_lanes_that_is_not_a_motorway = new RoadSegmentMorphology(102);
        public static readonly RoadSegmentMorphology Road_consisting_of_one_roadway = new RoadSegmentMorphology(103);
        public static readonly RoadSegmentMorphology Traffic_circle = new RoadSegmentMorphology(104);
        public static readonly RoadSegmentMorphology Special_traffic_situation = new RoadSegmentMorphology(105);
        public static readonly RoadSegmentMorphology Traffic_square = new RoadSegmentMorphology(106);
        public static readonly RoadSegmentMorphology Entry_or_exit_ramp_belonging_to_a_grade_separated_junction = new RoadSegmentMorphology(107);
        public static readonly RoadSegmentMorphology Entry_or_exit_ramp_belonging_to_a_level_junction = new RoadSegmentMorphology(108);
        public static readonly RoadSegmentMorphology Parallel_road = new RoadSegmentMorphology(109);
        public static readonly RoadSegmentMorphology Frontage_road = new RoadSegmentMorphology(110);
        public static readonly RoadSegmentMorphology Entry_or_exit_of_a_car_park = new RoadSegmentMorphology(111);
        public static readonly RoadSegmentMorphology Entry_or_exit_of_a_service = new RoadSegmentMorphology(112);
        public static readonly RoadSegmentMorphology Pedestrain_zone = new RoadSegmentMorphology(113);
        public static readonly RoadSegmentMorphology Walking_or_cycling_path_not_accessible_to_other_vehicles = new RoadSegmentMorphology(114);
        public static readonly RoadSegmentMorphology Tramway_not_accessible_to_other_vehicles = new RoadSegmentMorphology(116);
        public static readonly RoadSegmentMorphology Service_road = new RoadSegmentMorphology(120);
        public static readonly RoadSegmentMorphology Primitive_road = new RoadSegmentMorphology(125);
        public static readonly RoadSegmentMorphology Ferry = new RoadSegmentMorphology(130);

        public static readonly RoadSegmentMorphology[] All = {
            Unknown,
            Motorway,
            Road_with_separate_lanes_that_is_not_a_motorway,
            Road_consisting_of_one_roadway,
            Traffic_circle,
            Special_traffic_situation,
            Traffic_square,
            Entry_or_exit_ramp_belonging_to_a_grade_separated_junction,
            Entry_or_exit_ramp_belonging_to_a_level_junction,
            Parallel_road,
            Frontage_road,
            Entry_or_exit_of_a_car_park,
            Entry_or_exit_of_a_service,
            Pedestrain_zone,
            Walking_or_cycling_path_not_accessible_to_other_vehicles,
            Tramway_not_accessible_to_other_vehicles,
            Service_road,
            Primitive_road,
            Ferry
        };

        private readonly int _value;

        private RoadSegmentMorphology(int value)
        {
            _value = value;
        }

        public static bool TryParse(int value, out RoadSegmentMorphology parsed)
        {
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentMorphology Parse(int value)
        {
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road segment morphology.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentMorphology other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentMorphology type && Equals(type);
        public override int GetHashCode() => _value;
        public override string ToString() => _value.ToString();
        public int ToInt32() => _value;
        public static implicit operator int(RoadSegmentMorphology instance) => instance.ToInt32();
        public static bool operator ==(RoadSegmentMorphology left, RoadSegmentMorphology right) => Equals(left, right);
        public static bool operator !=(RoadSegmentMorphology left, RoadSegmentMorphology right) => !Equals(left, right);
    }
}