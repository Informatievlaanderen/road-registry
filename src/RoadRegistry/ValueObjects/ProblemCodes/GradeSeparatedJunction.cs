namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class GradeSeparatedJunction
    {
        public static readonly ProblemCode NotFound = new("GradeSeparatedJunctionNotFound");
        public static readonly ProblemCode LowerSegmentMissing = new("GradeSeparatedJunctionLowerRoadSegmentMissing");
        public static readonly ProblemCode UpperSegmentMissing = new("GradeSeparatedJunctionUpperRoadSegmentMissing");
        public static readonly ProblemCode UpperAndLowerDoNotIntersect = new("GradeSeparatedJunctionUpperAndLowerRoadSegmentDoNotIntersect");
        public static readonly ProblemCode NotUnique = new("GradeSeparatedJunctionNotUnique");
        public static readonly ProblemCode TemporaryIdNotUnique = new("GradeSeparatedJunctionTemporaryIdNotUnique");
        public static readonly ProblemCode NoRoadSegmentSpecified = new("GradeSeparatedJunctionNoRoadSegmentSpecified");
        public static readonly ProblemCode UpperEqualsLowerRoadSegment = new("GradeSeparatedJunctionUpperEqualsLowerRoadSegment");
        public static readonly ProblemCode DoesNotExist = new("GradeSeparatedJunctionDoesNotExist");
        public static readonly ProblemCode IsRemoved = new("GradeSeparatedJunctionIsRemoved");
        public static readonly ProblemCode RoadSegmentDoesNotBelong = new("RoadSegmentDoesNotBelongToGradeSeparatedJunction");
    }
}
