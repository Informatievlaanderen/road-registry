namespace RoadRegistry.ValueObjects.ProblemCodes;

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
        public static readonly ProblemCode IdsNotUnique = new("RoadSegmentIdsNotUnique");
        public static readonly ProblemCode NotRemovedBecauseCategoryIsInvalid = new("RoadSegmentNotRemovedBecauseCategoryIsInvalid");
        public static readonly ProblemCode TemporaryIdNotUnique = new("RoadSegmentTemporaryIdNotUnique");

        public static class AccessRestriction
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentAccessRestrictionNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentAccessRestrictionIsRequired");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentAccessRestrictionFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentAccessRestrictionFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentAccessRestrictionHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentAccessRestrictionHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentAccessRestrictionNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentAccessRestrictionToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentAccessRestrictionValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentAccessRestrictionLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentAccessRestrictionAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class BikeAccess
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentBikeAccessNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentBikeAccessIsRequired");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentBikeAccessAttributeFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentBikeAccessAttributeFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentBikeAccessAttributeHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentBikeAccessAttributeHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentBikeAccessAttributeNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentBikeAccessAttributeToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentBikeAccessAttributeValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentBikeAccessAttributeLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentBikeAccessAttributeAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class Category
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentCategoryNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentCategoryIsRequired");
            public static readonly ProblemCode NotChangedBecauseCurrentIsNewerVersion = new("RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentCategoryFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentCategoryFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentCategoryHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentCategoryHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentCategoryNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentCategoryToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentCategoryValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentCategoryLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentCategoryAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class CarAccess
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentCarAccessNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentCarAccessIsRequired");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentCarAccessAttributeFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentCarAccessAttributeFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentCarAccessAttributeHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentCarAccessAttributeHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentCarAccessAttributeNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentCarAccessAttributeToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentCarAccessAttributeValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentCarAccessAttributeLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentCarAccessAttributeAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class EuropeanRoads
        {
            public static readonly ProblemCode NotUnique = new("RoadSegmentEuropeanRoadsNotUnique");
        }

        public static class EuropeanRoadNumber
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentEuropeanRoadNumberNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentEuropeanRoadNumberIsRequired");
        }

        public static class NationalRoads
        {
            public static readonly ProblemCode NotUnique = new("RoadSegmentNationalRoadsNotUnique");
        }

        public static class NationalRoadNumber
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentNationalRoadNumberNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentNationalRoadNumberIsRequired");
        }

        public static class NumberedRoads
        {
            public static readonly ProblemCode NotUnique = new("RoadSegmentNumberedRoadsNotUnique");
        }

        public static class NumberedRoadNumber
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentNumberedRoadNumberNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentNumberedRoadNumberIsRequired");
        }

        public static class NumberedRoadDirection
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentNumberedRoadDirectionNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentNumberedRoadDirectionIsRequired");
        }

        public static class NumberedRoadOrdinal
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentNumberedRoadOrdinalNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentNumberedRoadOrdinalIsRequired");
        }

        public static class Geometry
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentGeometryNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentGeometryIsRequired");
            public static readonly ProblemCode LengthIsZero = new("RoadSegmentGeometryLengthIsZero");
            public static readonly ProblemCode LengthLessThanMinimum = new("RoadSegmentGeometryLengthLessThanMinimum");
            public static readonly ProblemCode LengthTooLong = new("RoadSegmentGeometryLengthTooLong");
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
            public static readonly ProblemCode NotKnown = new("RoadSegmentMaintenanceAuthorityNotKnown");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentMaintenanceAuthorityFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentMaintenanceAuthorityFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentMaintenanceAuthorityHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentMaintenanceAuthorityHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentMaintenanceAuthorityNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentMaintenanceAuthorityToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentMaintenanceAuthorityValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentMaintenanceAuthorityLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentMaintenanceAuthorityAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class Morphology
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentMorphologyNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentMorphologyIsRequired");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentMorphologyFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentMorphologyFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentMorphologyHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentMorphologyHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentMorphologyNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentMorphologyToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentMorphologyValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentMorphologyLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentMorphologyAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class PedestrianAccess
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentPedestrianAccessNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentPedestrianAccessIsRequired");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentPedestrianAccessAttributeFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentPedestrianAccessAttributeFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentPedestrianAccessAttributeHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentPedestrianAccessAttributeHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentPedestrianAccessAttributeNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentPedestrianAccessAttributeToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentPedestrianAccessAttributeValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentPedestrianAccessAttributeLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentPedestrianAccessAttributeAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class Status
        {
            public static readonly ProblemCode NotValid = new("RoadSegmentStatusNotValid");
            public static readonly ProblemCode IsRequired = new("RoadSegmentStatusIsRequired");

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentStatusFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentStatusFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentStatusHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentStatusHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentStatusNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentStatusToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentStatusValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentStatusLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentStatusAnotherSegmentFoundBesidesTheGlobalSegment")
            };
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

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentSurfaceAttributeFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentSurfaceAttributeFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentSurfaceAttributeHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentSurfaceAttributeHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentSurfaceAttributeNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentSurfaceAttributeToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentSurfaceAttributeValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentSurfaceAttributeLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentSurfaceAttributeAnotherSegmentFoundBesidesTheGlobalSegment")
            };
        }

        public static class StreetName
        {
            public static readonly ProblemCode NotProposedOrCurrent = new("RoadSegmentStreetNameNotProposedOrCurrent");

            public static class Left
            {
                public static readonly ProblemCode NotLinked = new("RoadSegmentStreetNameLeftNotLinked");
                public static readonly ProblemCode NotUnlinked = new("RoadSegmentStreetNameLeftNotUnlinked");
                public static readonly ProblemCode NotValid = new("RoadSegmentStreetNameLeftNotValid");
                public static readonly ProblemCode NotProposedOrCurrent = new("RoadSegmentStreetNameLeftNotProposedOrCurrent");
                public static readonly ProblemCode NotFound = new("RoadSegmentStreetNameLeftNotFound");
            }

            public static class Right
            {
                public static readonly ProblemCode NotLinked = new("RoadSegmentStreetNameRightNotLinked");
                public static readonly ProblemCode NotUnlinked = new("RoadSegmentStreetNameRightNotUnlinked");
                public static readonly ProblemCode NotValid = new("RoadSegmentStreetNameRightNotValid");
                public static readonly ProblemCode NotProposedOrCurrent = new("RoadSegmentStreetNameRightNotProposedOrCurrent");
                public static readonly ProblemCode NotFound = new("RoadSegmentStreetNameRightNotFound");
            }

            public static readonly DynamicAttributeProblemCodes DynamicAttributeProblemCodes = new()
            {
                FromOrToPositionIsNull = new("RoadSegmentStreetNameFromOrToPositionIsNull"),
                FromPositionNotEqualToZero = new("RoadSegmentStreetNameFromPositionNotEqualToZero"),
                HasCountOfZero = new("RoadSegmentStreetNameHasCountOfZero"),
                HasLengthOfZero = new("RoadSegmentStreetNameHasLengthOfZero"),
                NotAdjacent = new("RoadSegmentStreetNameNotAdjacent"),
                ToPositionNotEqualToLength = new("RoadSegmentStreetNameToPositionNotEqualToLength"),
                ValueNotUniqueWithinSegment = new("RoadSegmentStreetNameValueNotUniqueWithinSegment"),
                LeftOrRightNotAllowedWhenUsingBoth = new("RoadSegmentStreetNameLeftOrRightNotAllowedWhenUsingBoth"),
                AnotherSegmentFoundBesidesTheGlobalSegment = new("RoadSegmentStreetNameAnotherSegmentFoundBesidesTheGlobalSegment")
            };
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

        public sealed record DynamicAttributeProblemCodes
        {
            public required ProblemCode FromOrToPositionIsNull { get; init; }
            public required ProblemCode FromPositionNotEqualToZero { get; init; }
            public required ProblemCode HasCountOfZero { get; init; }
            public required ProblemCode HasLengthOfZero { get; init; }
            public required ProblemCode NotAdjacent { get; init; }
            public required ProblemCode ToPositionNotEqualToLength { get; init; }
            public required ProblemCode ValueNotUniqueWithinSegment { get; init; }
            public required ProblemCode LeftOrRightNotAllowedWhenUsingBoth { get; init; }
            public required ProblemCode AnotherSegmentFoundBesidesTheGlobalSegment { get; init; }
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
