namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadNode
    {
        public static readonly ProblemCode NotFound = new("RoadNodeNotFound");
        public static readonly ProblemCode NotConnectedToAnySegment = new("RoadNodeNotConnectedToAnySegment");
        public static readonly ProblemCode TemporaryIdNotUnique = new("RoadNodeTemporaryIdNotUnique");
        public static readonly ProblemCode TooClose = new("RoadNodeTooClose");
        public static readonly ProblemCode TypeMismatch = new("RoadNodeTypeMismatch");

        public static class Geometry
        {
            public static readonly ProblemCode Taken = new("RoadNodeGeometryTaken");
        }

        public static class Fake
        {
            public static readonly ProblemCode ConnectedSegmentsDoNotDiffer = new("FakeRoadNodeConnectedSegmentsDoNotDiffer");
        }
    }
}
