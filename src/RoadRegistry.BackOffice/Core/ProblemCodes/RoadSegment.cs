namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadSegment
    {
        public static readonly ProblemCode NotFound = new("RoadSegmentNotFound");
        public static readonly ProblemCode OutlinedNotFound = new("RoadSegmentOutlinedNotFound");
        public static readonly ProblemCode ChangeAttributesRequestNull = new("RoadSegmentChangeAttributesRequestNull");
        public static readonly ProblemCode ChangeAttributesAttributeNotValid = new("RoadSegmentChangeAttributesAttributeNotValid");
        public static readonly ProblemCode IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction = new("IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction");
        public static readonly ProblemCode LowerMissing = new("LowerRoadSegmentMissing");
        public static readonly ProblemCode Missing = new("RoadSegmentMissing");
        public static readonly ProblemCode UpperMissing = new("UpperRoadSegmentMissing");
        public static readonly ProblemCode UpperAndLowerDoNotIntersect = new("UpperAndLowerRoadSegmentDoNotIntersect");

        public static class AccessRestriction
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentAccessRestrictionNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentAccessRestrictionIsRequired");
        }

        public static class Category
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentCategoryNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentCategoryIsRequired");
        }

        public static class Geometry
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentGeometryNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentGeometryIsRequired");
            public static readonly ProblemCode LengthIsZero = new("RoadSegmentGeometryLengthIsZero");
            public static readonly ProblemCode LengthLessThanMinimum = new("RoadSegmentGeometryLengthLessThanMinimum");
            public static readonly ProblemCode SelfIntersects = new("RoadSegmentGeometrySelfIntersects");
            public static readonly ProblemCode SelfOverlaps = new("RoadSegmentGeometrySelfOverlaps");
            public static readonly ProblemCode SridNotValid = new("RoadSegmentGeometrySridNotValid");
            public static readonly ProblemCode Taken = new("RoadSegmentGeometryTaken");
        }

        public static class GeometryDrawMethod
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentGeometryDrawMethodNotValid");
            public static readonly ProblemCode NotOutlined = new("RoadSegmentGeometryDrawMethodNotOutlined");
        }

        public static class Lane
        {
            public static readonly ProblemCode IsRequired = new("RoadSegmentLaneIsRequired");
            public static readonly ProblemCode FromPositionNotEqualToZero = new("RoadSegmentLaneAttributeFromPositionNotEqualToZero");
            public static readonly ProblemCode GreaterThanZero = new("RoadSegmentLaneGreaterThanZero");
            public static readonly ProblemCode LessThanOrEqualToMaximum = new("RoadSegmentLaneLessThanOrEqualToMaximum");
            public static readonly ProblemCode HasLengthOfZero = new("RoadSegmentLaneAttributeHasLengthOfZero");
            public static readonly ProblemCode NotAdjacent = new("RoadSegmentLaneAttributesNotAdjacent");
            public static readonly ProblemCode ToPositionNotEqualToLength = new("RoadSegmentLaneAttributeToPositionNotEqualToLength");
        }

        public static class Lanes
        {
            public static readonly ProblemCode CountGreaterThanOne = new("RoadSegmentLanesCountGreaterThanOne");
            public static readonly ProblemCode HasCountOfZero = new("RoadSegmentLanesHasCountOfZero");
        }
        
        public static class LaneCount
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentLaneCountNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentLaneCountIsRequired");
        }
        
        public static class LaneDirection
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentLaneDirectionNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentLaneDirectionIsRequired");
        }

        public static class MaintenanceAuthority
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentMaintenanceAuthorityNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentMaintenanceAuthorityIsRequired");
        }

        public static class Morphology
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentMorphologyNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentMorphologyIsRequired");
        }

        public static class Status
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentStatusNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentStatusIsRequired");
        }

        public static class Width
        {
            public static readonly ProblemCode FromPositionNotEqualToZero = new("RoadSegmentWidthAttributeFromPositionNotEqualToZero");
            public static readonly ProblemCode HasLengthOfZero = new("RoadSegmentWidthHasLengthOfZero");
            public static readonly ProblemCode IsRequired = new("RoadSegmentWidthIsRequired");
            public static readonly ProblemCode NotValid = new("RoadSegmentWidthNotValid");
            public static readonly ProblemCode NotAdjacent = new("RoadSegmentWidthAttributesNotAdjacent");
            public static readonly ProblemCode ToPositionNotEqualToLength = new("RoadSegmentWidthAttributeToPositionNotEqualToLength");
            public static readonly ProblemCode LessThanOrEqualToMaximum = new("RoadSegmentWidthLessThanOrEqualToMaximum");
        }

        public static class Widths
        {
            public static readonly ProblemCode CountGreaterThanOne = new("RoadSegmentWidthsCountGreaterThanOne");
            public static readonly ProblemCode HasCountOfZero = new("RoadSegmentWidthsHasCountOfZero");
        }

        public static class Surface
        {
            public static readonly ProblemCode FromPositionNotEqualToZero = new("RoadSegmentSurfaceAttributeFromPositionNotEqualToZero");
            public static readonly ProblemCode HasLengthOfZero = new("RoadSegmentSurfaceAttributeHasLengthOfZero");
            public static readonly ProblemCode NotAdjacent = new("RoadSegmentSurfaceAttributesNotAdjacent");
            public static readonly ProblemCode ToPositionNotEqualToLength = new("RoadSegmentSurfaceAttributeToPositionNotEqualToLength");
        }

        public static class Surfaces
        {
            public static readonly ProblemCode CountGreaterThanOne = new("RoadSegmentSurfacesCountGreaterThanOne");
            public static readonly ProblemCode HasCountOfZero = new("RoadSegmentSurfacesHasCountOfZero");
        }

        public static class SurfaceType
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentSurfaceTypeNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentSurfaceTypeIsRequired");
        }

        public static class StreetName
        {
            public static readonly ProblemCode NotProposedOrCurrent = new("RoadSegmentStreetNameNotProposedOrCurrent");

            public static class Left
            {
                public static readonly ProblemCode NotLinked = new("RoadSegmentStreetNameLeftNotLinked");
                public static readonly ProblemCode NotUnlinked = new("RoadSegmentStreetNameLeftNotUnlinked");
            }

            public static class Right
            {
                public static readonly ProblemCode NotLinked = new("RoadSegmentStreetNameRightNotLinked");
                public static readonly ProblemCode NotUnlinked = new("RoadSegmentStreetNameRightNotUnlinked");
            }
        }

        public static class StartNode
        {
            public static readonly ProblemCode Missing = new("RoadSegmentStartNodeMissing");
            public static readonly ProblemCode RefersToRemovedNode = new("RoadSegmentStartNodeRefersToRemovedNode");
        }

        public static class EndNode
        {
            public static readonly ProblemCode Missing = new("RoadSegmentEndNodeMissing");
            public static readonly ProblemCode RefersToRemovedNode = new("RoadSegmentEndNodeRefersToRemovedNode");
        }

        public static class StartPoint
        {
            public static readonly ProblemCode DoesNotMatchNodeGeometry = new("RoadSegmentStartPointDoesNotMatchNodeGeometry");
            public static readonly ProblemCode MeasureValueNotEqualToZero = new("RoadSegmentStartPointMeasureValueNotEqualToZero");
        }

        public static class Point
        {
            public static readonly ProblemCode MeasureValueDoesNotIncrease = new("RoadSegmentPointMeasureValueDoesNotIncrease");
            public static readonly ProblemCode MeasureValueOutOfRange = new("RoadSegmentPointMeasureValueOutOfRange");
        }

        public static class EndPoint
        {
            public static readonly ProblemCode DoesNotMatchNodeGeometry = new("RoadSegmentEndPointDoesNotMatchNodeGeometry");
            public static readonly ProblemCode MeasureValueNotEqualToLength = new("RoadSegmentEndPointMeasureValueNotEqualToLength");
        }
    }

    public static class Type
    {
        public static readonly ProblemCode NotValid = new("TypeNotValid");
        public static readonly ProblemCode IsRequired = new("TypeIsRequired");
    }

    public static class Count
    {
        public static readonly ProblemCode IsRequired = new("CountIsRequired");
        public static readonly ProblemCode NotValid = new("CountNotValid");
    }

    public static class Direction
    {
        public static readonly ProblemCode IsRequired = new("DirectionIsRequired");
        public static readonly ProblemCode NotValid = new("DirectionNotValid");
    }

    public static class FromPosition
    {
        public static readonly ProblemCode IsRequired = new("FromPositionIsRequired");
        public static readonly ProblemCode NotEqualToZero = new("FromPositionNotEqualToZero");
        public static readonly ProblemCode NotValid = new("FromPositionNotValid");
    }

    public static class ToPosition
    {
        public static readonly ProblemCode IsRequired = new("ToPositionIsRequired");
        public static readonly ProblemCode NotValid = new("ToPositionNotValid");
        public static readonly ProblemCode NotEqualToNextFromPosition = new("ToPositionNotEqualToNextFromPosition");
        public static readonly ProblemCode LessThanOrEqualFromPosition = new("ToPositionLessThanOrEqualFromPosition");
    }

    public static class Width
    {
        public static readonly ProblemCode IsRequired = new("WidthIsRequired");
        public static readonly ProblemCode NotValid = new("WidthNotValid");
    }

    public static class RoadSegments
    {
        public static readonly ProblemCode NotFound = new("RoadSegmentsNotFound");
    }
}
