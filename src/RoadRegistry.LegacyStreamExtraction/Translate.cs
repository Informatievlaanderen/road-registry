using System;
using RoadRegistry.Events;

namespace RoadRegistry.LegacyImporter
{
    internal static class Translate
    {
        private static readonly RoadNodeType[] RoadNodeTypeIndex =
            {
                RoadNodeType.RealNode,
                RoadNodeType.FakeNode,
                RoadNodeType.EndNode,
                RoadNodeType.MiniRoundabout,
                RoadNodeType.TurnLoopNode
            };

        public static RoadNodeType ToRoadNodeType(int code)
        {
            var index = code - 1;
            if (index < 0 || index >= RoadNodeTypeIndex.Length)
                throw new InvalidOperationException($"Road node type {code} can not be translated.");

            return RoadNodeTypeIndex[index];
        }

        private static readonly RoadSegmentGeometryDrawMethod[] RoadSegmentGeometryDrawMethodIndex =
            {
                RoadSegmentGeometryDrawMethod.Outlined,
                RoadSegmentGeometryDrawMethod.Measured,
                RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications
            };

        public static RoadSegmentGeometryDrawMethod ToRoadSegmentGeometryDrawMethod(int code)
        {
            var index = code - 1;
            if (index < 0 || index >= RoadSegmentGeometryDrawMethodIndex.Length)
                throw new InvalidOperationException($"Road node method {code} can not be translated.");

            return RoadSegmentGeometryDrawMethodIndex[index];
        }

        public static RoadSegmentMorphology ToRoadSegmentMorphology(int code)
        {
            var result = RoadSegmentMorphology.Unknown;

            switch (code)
            {
                //case -8: result = RoadSegmentMorphology.Unknown; break;

                case 101: result = RoadSegmentMorphology.Motorway; break;
                case 102: result = RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway; break;
                case 103: result = RoadSegmentMorphology.Road_consisting_of_one_roadway; break;

                case 104: result = RoadSegmentMorphology.Traffic_circle; break;
                case 105: result = RoadSegmentMorphology.Special_traffic_situation; break;
                case 106: result = RoadSegmentMorphology.Traffic_square; break;

                case 107: result = RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction; break;
                case 108: result = RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_at_grade_junction; break;
                case 109: result = RoadSegmentMorphology.Parallel_road; break;

                case 110: result = RoadSegmentMorphology.Frontage_road; break;
                case 111: result = RoadSegmentMorphology.Entry_or_exit_of_a_car_park; break;
                case 112: result = RoadSegmentMorphology.Entry_or_exit_of_a_service; break;


                case 113: result = RoadSegmentMorphology.Pedestrain_zone; break;
                case 114: result = RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles; break;
                case 116: result = RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles; break;

                case 120: result = RoadSegmentMorphology.Service_road; break;
                case 125: result = RoadSegmentMorphology.Primitive_road; break;
                case 130: result = RoadSegmentMorphology.Ferry; break;
            }

            return result;
        }

        public static RoadSegmentStatus ToRoadSegmentStatus(int code)
        {
            var result = RoadSegmentStatus.Unknown;

            switch (code)
            {
                //case -8: result = RoadSegmentStatus.Unknown; break;
                case 1: result = RoadSegmentStatus.PermitRequested; break;
                case 2: result = RoadSegmentStatus.BuildingPermitGranted; break;
                case 3: result = RoadSegmentStatus.UnderConstruction; break;
                case 4: result = RoadSegmentStatus.InUse; break;
                case 5: result = RoadSegmentStatus.OutOfUse; break;
            }

            return result;
        }

        public static RoadSegmentCategory ToRoadSegmentCategory(string code)
        {
            var result = RoadSegmentCategory.Unknown;

            switch (code.ToUpperInvariant())
            {
                //case "-8": result = RoadSegmentCategory.Unknown; break;
                case "-9": result = RoadSegmentCategory.NotApplicable; break;
                case "H": result = RoadSegmentCategory.MainRoad; break;
                case "L": result = RoadSegmentCategory.LocalRoad; break;
                case "L1": result = RoadSegmentCategory.LocalRoadType1; break;
                case "L2": result = RoadSegmentCategory.LocalRoadType2; break;
                case "L3": result = RoadSegmentCategory.LocalRoadType3; break;
                case "PI": result = RoadSegmentCategory.PrimaryRoadI; break;
                case "PII": result = RoadSegmentCategory.PrimaryRoadII; break;
                case "PII-1": result = RoadSegmentCategory.PrimaryRoadIIType1; break;
                case "PII-2": result = RoadSegmentCategory.PrimaryRoadIIType2; break;
                case "PII-3": result = RoadSegmentCategory.PrimaryRoadIIType3; break;
                case "PII-4": result = RoadSegmentCategory.PrimaryRoadIIType4; break;
                case "S": result = RoadSegmentCategory.SecondaryRoad; break;
                case "S1": result = RoadSegmentCategory.SecondaryRoadType1; break;
                case "S2": result = RoadSegmentCategory.SecondaryRoadType2; break;
                case "S3": result = RoadSegmentCategory.SecondaryRoadType3; break;
                case "S4": result = RoadSegmentCategory.SecondaryRoadType4; break;
            }

            return result;
        }

        private static readonly RoadSegmentAccessRestriction[] RoadSegmentAccessRestrictionIndex =
            {
                RoadSegmentAccessRestriction.PublicRoad,
                RoadSegmentAccessRestriction.PhysicallyImpossible,
                RoadSegmentAccessRestriction.LegallyForbidden,
                RoadSegmentAccessRestriction.PrivateRoad,
                RoadSegmentAccessRestriction.Seasonal,
                RoadSegmentAccessRestriction.Toll
            };

        public static RoadSegmentAccessRestriction ToRoadSegmentAccessRestriction(int code)
        {
            var index = code - 1;
            if (index < 0 || index >= RoadSegmentAccessRestrictionIndex.Length)
                throw new InvalidOperationException($"Road link access restriction {code} can not be translated.");

            return RoadSegmentAccessRestrictionIndex[index];
        }

        public static GradeSeparatedJunctionType ToGradeSeparatedJunctionType(int code)
        {
            var result = GradeSeparatedJunctionType.Unknown;

            switch (code)
            {
                //case -8: result = GradeSeparatedJunctionType.Unknown; break;
                case 1: result = GradeSeparatedJunctionType.Tunnel; break;
                case 2: result = GradeSeparatedJunctionType.Bridge; break;
            }

            return result;
        }

        public static LaneDirection ToLaneDirection(int code)
        {
            var result = LaneDirection.Unknown;

            switch (code)
            {
                //case -8: result = LaneDirection.Unknown; break;
                case 1: result = LaneDirection.Forward; break;
                case 2: result = LaneDirection.Backward; break;
                case 3: result = LaneDirection.Independent; break;
            }

            return result;
        }

        public static NumberedRoadSegmentDirection ToNumberedRoadSegmentDirection(int code)
        {
            var result = NumberedRoadSegmentDirection.Unknown;

            switch (code)
            {
                //case -8: result = NumberedRoadSegmentDirection.Unknown; break;
                case 1: result = NumberedRoadSegmentDirection.Forward; break;
                case 2: result = NumberedRoadSegmentDirection.Backward; break;
            }

            return result;
        }

        public static HardeningType ToHardeningType(int code)
        {
            var result = HardeningType.Unknown;

            switch (code)
            {
                case -9: result = HardeningType.NotApplicable; break;
                //case -8: result = HardeningType.Unknown; break;
                case 1: result = HardeningType.SolidHardening; break;
                case 2: result = HardeningType.LooseHardening; break;
            }

            return result;
        }

        public static ReferencePointType ToReferencePointType(int code)
        {
            var result = ReferencePointType.Unknown;

            switch (code)
            {
                //case -8: result = ReferencePointType.Unknown; break;
                case 1: result = ReferencePointType.KilometerMarker; break;
                case 2: result = ReferencePointType.HectometerMarker; break;
            }

            return result;
        }
    }
}
