namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadNode
    {
        public static ProblemCode NotFound = new("RoadNodeNotFound");
        public static ProblemCode NotConnectedToAnySegment = new("RoadNodeNotConnectedToAnySegment");
        public static ProblemCode TooClose = new("RoadNodeTooClose");
        public static ProblemCode TypeMismatch = new("RoadNodeTypeMismatch");

        public static class Geometry
        {
            public static ProblemCode Taken = new("RoadNodeGeometryTaken");
        }

        public static class Fake
        {
            public static ProblemCode ConnectedSegmentsDoNotDiffer = new("FakeRoadNodeConnectedSegmentsDoNotDiffer");
        }
    }
}
