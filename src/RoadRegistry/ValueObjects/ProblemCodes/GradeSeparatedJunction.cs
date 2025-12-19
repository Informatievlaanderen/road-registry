namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class GradeSeparatedJunction
    {
        public static readonly ProblemCode NotFound = new("GradeSeparatedJunctionNotFound");
        public static readonly ProblemCode LowerSegmentMissing = new("GradeSeparatedJunctionLowerRoadSegmentMissing");
        public static readonly ProblemCode UpperSegmentMissing = new("GradeSeparatedJunctionUpperRoadSegmentMissing");
        public static readonly ProblemCode UpperAndLowerDoNotIntersect = new("GradeSeparatedJunctionUpperAndLowerRoadSegmentDoNotIntersect");
    }
}
