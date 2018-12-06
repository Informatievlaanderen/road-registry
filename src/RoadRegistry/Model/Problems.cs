namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class Problems : IReadOnlyCollection<Problem>
    {
        public static Problems With(IRequestedChanges requestedChanges)
        {
            if (requestedChanges == null) throw new ArgumentNullException(nameof(requestedChanges));
            return new Problems(ImmutableList<Problem>.Empty, requestedChanges);
        }

        private readonly ImmutableList<Problem> _reasons;
        private readonly IRequestedChanges _requestedChanges;

        private Problems(ImmutableList<Problem> reasons, IRequestedChanges requestedChanges)
        {
            _reasons = reasons;
            _requestedChanges = requestedChanges;
        }

        public bool AreAcceptable() => _reasons.Count == 0 || _reasons.All(reason => reason is Warning);

        public int Count => _reasons.Count;

        public IEnumerator<Problem> GetEnumerator() => ((IEnumerable<Problem>)_reasons).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Problems RoadNodeIdTaken() => new Problems(_reasons.Add(new Error(nameof(RoadNodeIdTaken))), _requestedChanges);

        public Problems RoadSegmentIdTaken() => new Problems(_reasons.Add(new Error(nameof(RoadSegmentIdTaken))), _requestedChanges);

        public Problems RoadNodeGeometryTaken(RoadNodeId byOtherNode)
        {
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadNodeGeometryTaken),
                    new ProblemParameter(
                        "ByOtherNode",
                        (
                            _requestedChanges.TryResolveTemporary(byOtherNode, out var temporary)
                                ? temporary
                                : byOtherNode
                        ).ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadNodeTooClose(RoadNodeId toOtherNode)
        {
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadNodeTooClose),
                    new ProblemParameter(
                        "ToOtherNode",
                        (
                            _requestedChanges.TryResolveTemporary(toOtherNode, out var temporary)
                                ? temporary
                                : toOtherNode
                        ).ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        {
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadSegmentGeometryTaken),
                    new ProblemParameter(
                        "ByOtherSegment",
                        (
                            _requestedChanges.TryResolveTemporary(byOtherSegment, out var temporary)
                                ? temporary
                                : byOtherSegment
                        ).ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadNodeNotConnectedToAnySegment() =>
            new Problems(_reasons.Add(new Error(nameof(RoadNodeNotConnectedToAnySegment))), _requestedChanges);

        public Problems RoadSegmentStartNodeMissing() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentStartNodeMissing))), _requestedChanges);

        public Problems RoadSegmentEndNodeMissing() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentEndNodeMissing))), _requestedChanges);

        public Problems RoadSegmentGeometryLengthIsZero() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentGeometryLengthIsZero))), _requestedChanges);

        public Problems RoadSegmentStartPointDoesNotMatchNodeGeometry() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentStartPointDoesNotMatchNodeGeometry))), _requestedChanges);

        public Problems RoadSegmentEndPointDoesNotMatchNodeGeometry() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentEndPointDoesNotMatchNodeGeometry))), _requestedChanges);

        public Problems RoadNodeTypeMismatch(params RoadNodeType[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (types.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(types));
            return new Problems(_reasons.Add(
                new Error(
                    nameof(RoadNodeTypeMismatch),
                    Array.ConvertAll(types, type => new ProblemParameter("Expected", type.ToString())))), _requestedChanges);
        }

        public Problems RoadSegmentGeometrySelfOverlaps() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentGeometrySelfOverlaps))), _requestedChanges);

        public Problems RoadSegmentGeometrySelfIntersects() =>
            new Problems(_reasons.Add(new Error(nameof(RoadSegmentGeometrySelfIntersects))), _requestedChanges);

        public Problems FakeRoadNodeConnectedSegmentsDoNotDiffer(RoadSegmentId segment1, RoadSegmentId segment2)
        {
            return new Problems(
                _reasons.Add(
                    new Error(nameof(FakeRoadNodeConnectedSegmentsDoNotDiffer),
                        new ProblemParameter(
                            "SegmentId",
                            (
                                _requestedChanges.TryResolveTemporary(segment1, out var temporary1)
                                    ? temporary1
                                    : segment1
                            ).ToInt32().ToString()),
                        new ProblemParameter(
                            "SegmentId",
                            (
                                _requestedChanges.TryResolveTemporary(segment2, out var temporary2)
                                    ? temporary2
                                    : segment2
                            ).ToInt32().ToString()))),
                _requestedChanges);
        }
    }
}
