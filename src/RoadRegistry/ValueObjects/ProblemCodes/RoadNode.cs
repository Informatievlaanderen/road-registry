namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class RoadNode
    {
        public static readonly ProblemCode NotFound = new("RoadNodeNotFound");
        public static readonly ProblemCode NotCompletedInwinning = new("RoadNodeNotCompletedInwinning");
        public static readonly ProblemCode NotConnectedToAnySegment = new("RoadNodeNotConnectedToAnySegment");
        public static readonly ProblemCode TemporaryIdNotUnique = new("RoadNodeTemporaryIdNotUnique");
        public static readonly ProblemCode TooClose = new("RoadNodeTooClose");
        public static readonly ProblemCode TypeMismatch = new("RoadNodeTypeMismatch");
        public static readonly ProblemCode TypeV2Mismatch = new("RoadNodeTypeV2Mismatch");
        public static readonly ProblemCode IsNotAllowed = new("RoadNodeIsNotAllowed");

        public static class Remove
        {
            public static readonly ProblemCode DoesNotExist = new("RoadNodeRemoveDoesNotExist");
            public static readonly ProblemCode IsRemoved = new("RoadNodeRemoveIsRemoved");
            public static readonly ProblemCode CannotBeRemovedByMerging = new("RoadNodeCannotBeRemovedByMerging");
        }

        public static class ChangeAttributes
        {
            public static readonly ProblemCode NotFound = new("RoadNodeChangeAttributesNotFound");
            public static readonly ProblemCode IsRemoved = new("RoadNodeChangeAttributesIsRemoved");
        }

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
