namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class Reasons : IReadOnlyCollection<Reason>
    {
        public static readonly Reasons None = new Reasons(ImmutableList<Reason>.Empty);

        private readonly ImmutableList<Reason> _reasons;

        private Reasons(ImmutableList<Reason> reasons)
        {
            _reasons = reasons;
        }

        public bool AreAcceptable() => _reasons.All(reason => reason is Warning);

        public int Count => _reasons.Count;

        public IEnumerator<Reason> GetEnumerator() => ((IEnumerable<Reason>)_reasons).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Reasons BecauseRoadNodeIdTaken() => new Reasons(_reasons.Add(new Error("RoadNodeIdTaken")));

        public Reasons BecauseRoadSegmentIdTaken() => new Reasons(_reasons.Add(new Error("RoadSegmentIdTaken")));

        public Reasons BecauseRoadNodeGeometryTaken(RoadNodeId byOtherNode)
        {
            return new Reasons(_reasons.Add(
                new Error(
                    "RoadNodeGeometryTaken",
                    new ReasonParameter("ByOtherNode", byOtherNode.ToInt32().ToString()))));
        }

        public Reasons BecauseRoadNodeTooClose(RoadNodeId toOtherNode)
        {
            return new Reasons(_reasons.Add(
                new Error("RoadNodeTooClose",
                    new ReasonParameter("ToOtherNode", toOtherNode.ToInt32().ToString()))));
        }

        public Reasons BecauseRoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        {
            return new Reasons(_reasons.Add(
                new Error("RoadSegmentGeometryTaken",
                    new ReasonParameter("ByOtherSegment", byOtherSegment.ToInt32().ToString()))));
        }

        public Reasons BecauseRoadNodeNotConnectedToAnySegment() =>
            new Reasons(_reasons.Add(new Error("RoadNodeNotConnectedToAnySegment")));

        public Reasons BecauseRoadSegmentStartNodeMissing() =>
            new Reasons(_reasons.Add(new Error("RoadSegmentStartNodeMissing")));

        public Reasons BecauseRoadSegmentEndNodeMissing() =>
            new Reasons(_reasons.Add(new Error("RoadSegmentEndNodeMissing")));

        public Reasons BecauseRoadSegmentGeometryLengthIsZero() =>
            new Reasons(_reasons.Add(new Error("RoadSegmentGeometryLengthIsZero")));

        public Reasons BecauseRoadSegmentStartPointDoesNotMatchNodeGeometry() =>
            new Reasons(_reasons.Add(new Error("RoadSegmentStartPointDoesNotMatchNodeGeometry")));

        public Reasons BecauseRoadSegmentEndPointDoesNotMatchNodeGeometry() =>
            new Reasons(_reasons.Add(new Error("RoadSegmentEndPointDoesNotMatchNodeGeometry")));

        public Reasons BecauseRoadNodeTypeMismatch(params RoadNodeType[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (types.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(types));
            return new Reasons(_reasons.Add(
                new Error(
                    "RoadNodeTypeMismatch",
                    Array.ConvertAll(types, type => new ReasonParameter("Expected", type.ToString())))));
        }

        public Reasons BecauseRoadSegmentGeometrySelfOverlaps() =>
            new Reasons(_reasons.Add(new Error("RoadSegmentGeometrySelfOverlaps")));

        public Reasons BecauseRoadSegmentGeometrySelfIntersects() =>
            new Reasons(_reasons.Add(new Error("RoadSegmentGeometrySelfIntersects")));

        public Reasons BecauseFakeRoadNodeConnectedSegmentsDoNotDiffer(RoadSegmentId segment1, RoadSegmentId segment2)
        {
            return new Reasons(
                _reasons.Add(
                    new Error("FakeRoadNodeConnectedSegmentsDoNotDiffer",
                        new ReasonParameter("RoadSegmentId", segment1.ToString()),
                        new ReasonParameter("RoadSegmentId", segment2.ToString()))));
        }
    }
}
