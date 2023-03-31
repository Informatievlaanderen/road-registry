namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadSegment
    {
        public static ProblemCode NotFound = new("RoadSegmentNotFound");
        public static ProblemCode ChangeAttributesRequestNull = new("RoadSegmentChangeAttributesRequestNull");
        public static ProblemCode ChangeAttributesAttributeNotValid = new("RoadSegmentChangeAttributesAttributeNotValid");
        public static ProblemCode IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction = new("IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction");
        public static ProblemCode LowerMissing = new("LowerRoadSegmentMissing");
        public static ProblemCode Missing = new("RoadSegmentMissing");
        public static ProblemCode UpperMissing = new("UpperRoadSegmentMissing");
        public static ProblemCode UpperAndLowerDoNotIntersect = new("UpperAndLowerRoadSegmentDoNotIntersect");

        public static class AccessRestriction
        {
            public static ProblemCode NotValid = new("RoadSegmentAccessRestrictionNotValid");
            public static ProblemCode IsRequired = new("RoadSegmentAccessRestrictionIsRequired");
        }

        public static class Category
        {
            public static ProblemCode NotValid = new("RoadSegmentCategoryNotValid");
            public static ProblemCode IsRequired = new("RoadSegmentCategoryIsRequired");
        }

        public static class Geometry
        {
            public static ProblemCode LengthIsZero = new("RoadSegmentGeometryLengthIsZero");
            public static ProblemCode SelfIntersects = new("RoadSegmentGeometrySelfIntersects");
            public static ProblemCode SelfOverlaps = new("RoadSegmentGeometrySelfOverlaps");
            public static ProblemCode Taken = new("RoadSegmentGeometryTaken");
        }

        public static class GeometryDrawMethod
        {
            public static ProblemCode NotValid = new("RoadSegmentGeometryDrawMethodNotValid");
        }

        public static class Lane
        {
            public static ProblemCode IsRequired = new("RoadSegmentLaneIsRequired");
            public static ProblemCode FromPositionNotEqualToZero = new("RoadSegmentLaneAttributeFromPositionNotEqualToZero");
            public static ProblemCode GreaterThanZero = new("RoadSegmentLaneGreaterThanZero");
            public static ProblemCode HasLengthOfZero = new("RoadSegmentLaneAttributeHasLengthOfZero");
            public static ProblemCode NotAdjacent = new("RoadSegmentLaneAttributesNotAdjacent");
            public static ProblemCode ToPositionNotEqualToLength = new("RoadSegmentLaneAttributeToPositionNotEqualToLength");
        }

        public static class LaneDirection
        {
            public static ProblemCode NotValid = new("RoadSegmentLaneDirectionNotValid");
            public static ProblemCode IsRequired = new("RoadSegmentLaneDirectionIsRequired");
        }

        public static class MaintenanceAuthority
        {
            public static ProblemCode NotValid = new("RoadSegmentMaintenanceAuthorityNotValid");
            public static ProblemCode IsRequired = new("RoadSegmentMaintenanceAuthorityIsRequired");
        }

        public static class Morphology
        {
            public static ProblemCode NotValid = new("RoadSegmentMorphologyNotValid");
            public static ProblemCode IsRequired = new("RoadSegmentMorphologyIsRequired");
        }

        public static class Status
        {
            public static ProblemCode NotValid = new("RoadSegmentStatusNotValid");
            public static ProblemCode IsRequired = new("RoadSegmentStatusIsRequired");
        }

        public static class Width
        {
            public static ProblemCode FromPositionNotEqualToZero = new("RoadSegmentWidthAttributeFromPositionNotEqualToZero");
            public static ProblemCode GreaterThanZero = new("RoadSegmentWidthAttributeGreaterThanZero");
            public static ProblemCode HasLengthOfZero = new("RoadSegmentWidthHasLengthOfZero");
            public static ProblemCode IsRequired = new("RoadSegmentWidthIsRequired");
            public static ProblemCode NotAdjacent = new("RoadSegmentWidthAttributesNotAdjacent");
            public static ProblemCode NotValid = new("RoadSegmentWidthNotValid");
            public static ProblemCode ToPositionNotEqualToLength = new("RoadSegmentWidthAttributeToPositionNotEqualToLength");
        }

        public static class Surface
        {
            public static ProblemCode FromPositionNotEqualToZero = new("RoadSegmentSurfaceAttributeFromPositionNotEqualToZero");
            public static ProblemCode HasLengthOfZero = new("RoadSegmentSurfaceAttributeHasLengthOfZero");
            public static ProblemCode NotAdjacent = new("RoadSegmentSurfaceAttributesNotAdjacent");
            public static ProblemCode ToPositionNotEqualToLength = new("RoadSegmentSurfaceAttributeToPositionNotEqualToLength");
        }

        public static class SurfaceType
        {
            public static ProblemCode NotValid = new("RoadSegmentSurfaceTypNotValid");
            public static ProblemCode IsRequired = new("RoadSegmentSurfaceTypeIsRequired");
        }

        public static class StreetName
        {
            public static ProblemCode NotProposedOrCurrent = new("RoadSegmentStreetNameNotProposedOrCurrent");

            public static class Left
            {
                public static ProblemCode NotLinked = new("RoadSegmentStreetNameLeftNotLinked");
                public static ProblemCode NotUnlinked = new("RoadSegmentStreetNameLeftNotUnlinked");
            }

            public static class Right
            {
                public static ProblemCode NotLinked = new("RoadSegmentStreetNameRightNotLinked");
                public static ProblemCode NotUnlinked = new("RoadSegmentStreetNameRightNotUnlinked");
            }
        }

        public static class StartNode
        {
            public static ProblemCode Missing = new("RoadSegmentStartNodeMissing");
            public static ProblemCode RefersToRemovedNode = new("RoadSegmentStartNodeRefersToRemovedNode");
        }

        public static class EndNode
        {
            public static ProblemCode Missing = new("RoadSegmentEndNodeMissing");
            public static ProblemCode RefersToRemovedNode = new("RoadSegmentEndNodeRefersToRemovedNode");
        }

        public static class StartPoint
        {
            public static ProblemCode DoesNotMatchNodeGeometry = new("RoadSegmentStartPointDoesNotMatchNodeGeometry");
            public static ProblemCode MeasureValueNotEqualToZero = new("RoadSegmentStartPointMeasureValueNotEqualToZero");
        }

        public static class Point
        {
            public static ProblemCode MeasureValueDoesNotIncrease = new("RoadSegmentPointMeasureValueDoesNotIncrease");
            public static ProblemCode MeasureValueOutOfRange = new("RoadSegmentPointMeasureValueOutOfRange");
        }

        public static class EndPoint
        {
            public static ProblemCode DoesNotMatchNodeGeometry = new("RoadSegmentEndPointDoesNotMatchNodeGeometry");
            public static ProblemCode MeasureValueNotEqualToLength = new("RoadSegmentEndPointMeasureValueNotEqualToLength");
        }
    }

    public static class RoadSegments
    {
        public static ProblemCode NotFound = new("RoadSegmentsNotFound");
    }
}
