namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class Problems : IReadOnlyCollection<Problem>
    {
        public static readonly Problems None = new Problems(ImmutableList<Problem>.Empty);

        private readonly ImmutableList<Problem> _reasons;

        private Problems(ImmutableList<Problem> reasons)
        {
            _reasons = reasons;
        }

        public bool AreAcceptable() => _reasons.All(reason => reason is Warning);

        public int Count => _reasons.Count;

        public IEnumerator<Problem> GetEnumerator() => ((IEnumerable<Problem>)_reasons).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Problems RoadNodeIdTaken() => new Problems(_reasons.Add(new Error(nameof(RoadNodeIdTaken))));

        public Problems RoadSegmentIdTaken() => new Problems(_reasons.Add(new Error(nameof(RoadSegmentIdTaken))));

        public Problems RoadNodeGeometryTaken(RoadNodeId byOtherNode)
        {
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadNodeGeometryTaken),
                    new ProblemParameter("ByOtherNode", byOtherNode.ToInt32().ToString()))));
        }

        public Problems RoadNodeTooClose(RoadNodeId toOtherNode)
        {
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadNodeTooClose),
                    new ProblemParameter("ToOtherNode", toOtherNode.ToInt32().ToString()))));
        }

        public Problems RoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        {
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadSegmentGeometryTaken),
                    new ProblemParameter("ByOtherSegment", byOtherSegment.ToInt32().ToString()))));
        }

        public Problems RoadNodeNotConnectedToAnySegment() =>
            new Problems(_reasons.Add(new Error(nameof(RoadNodeNotConnectedToAnySegment))));

        public Problems RoadSegmentStartNodeMissing() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentStartNodeMissing))));

        public Problems RoadSegmentEndNodeMissing() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentEndNodeMissing))));

        public Problems RoadSegmentGeometryLengthIsZero() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentGeometryLengthIsZero))));

        public Problems RoadSegmentStartPointDoesNotMatchNodeGeometry() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentStartPointDoesNotMatchNodeGeometry))));

        public Problems RoadSegmentEndPointDoesNotMatchNodeGeometry() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentEndPointDoesNotMatchNodeGeometry))));

        public Problems RoadNodeTypeMismatch(params RoadNodeType[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (types.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(types));
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadNodeTypeMismatch),
                    Array.ConvertAll(types, type => new ProblemParameter("Expected", type.ToString())))));
        }

        public Problems RoadSegmentGeometrySelfOverlaps() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentGeometrySelfOverlaps))));

        public Problems RoadSegmentGeometrySelfIntersects() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentGeometrySelfIntersects))));

        public Problems FakeRoadNodeConnectedSegmentsDoNotDiffer(RoadSegmentId segment1, RoadSegmentId segment2)
        {
            return new Problems(
                _reasons.Add(
                    new Error(nameof(FakeRoadNodeConnectedSegmentsDoNotDiffer),
                        new ProblemParameter("SegmentId", segment1.ToString()),
                        new ProblemParameter("SegmentId", segment2.ToString()))));
        }
    }
}
