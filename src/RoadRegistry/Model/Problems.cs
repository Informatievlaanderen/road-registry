namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;

    internal class Problems : IReadOnlyCollection<Problem>
    {
        public static Problems With(IRequestedChanges requestedChanges)
        {
            if (requestedChanges == null) throw new ArgumentNullException(nameof(requestedChanges));
            return new Problems(ImmutableList<Problem>.Empty, requestedChanges);
        }

        private readonly ImmutableList<Problem> _problems;
        private readonly IRequestedChanges _requestedChanges;

        private Problems(ImmutableList<Problem> problems, IRequestedChanges requestedChanges)
        {
            _problems = problems;
            _requestedChanges = requestedChanges;
        }

        public bool AreAcceptable() => _problems.Count == 0 || _problems.All(reason => reason is Warning);

        public int Count => _problems.Count;

        public IEnumerator<Problem> GetEnumerator() => ((IEnumerable<Problem>)_problems).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Problems RoadNodeIdTaken() => new Problems(_problems.Add(new Error(nameof(RoadNodeIdTaken))), _requestedChanges);

        public Problems RoadSegmentIdTaken() => new Problems(_problems.Add(new Error(nameof(RoadSegmentIdTaken))), _requestedChanges);

        public Problems RoadNodeGeometryTaken(RoadNodeId byOtherNode)
        {
            return new Problems(_problems.Add(
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
            return new Problems(_problems.Add(
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
            return new Problems(_problems.Add(
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
            new Problems(_problems.Add(new Error(nameof(RoadNodeNotConnectedToAnySegment))), _requestedChanges);

        public Problems RoadSegmentStartNodeMissing() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentStartNodeMissing))), _requestedChanges);

        public Problems RoadSegmentEndNodeMissing() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentEndNodeMissing))), _requestedChanges);

        public Problems RoadSegmentGeometryLengthIsZero() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentGeometryLengthIsZero))), _requestedChanges);

        public Problems RoadSegmentStartPointDoesNotMatchNodeGeometry() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentStartPointDoesNotMatchNodeGeometry))), _requestedChanges);

        public Problems RoadSegmentEndPointDoesNotMatchNodeGeometry() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentEndPointDoesNotMatchNodeGeometry))), _requestedChanges);

        public Problems RoadNodeTypeMismatch(params RoadNodeType[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (types.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(types));
            return new Problems(_problems.Add(
                new Error(
                    nameof(RoadNodeTypeMismatch),
                    Array.ConvertAll(types, type => new ProblemParameter("Expected", type.ToString())))), _requestedChanges);
        }

        public Problems RoadSegmentGeometrySelfOverlaps() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentGeometrySelfOverlaps))), _requestedChanges);

        public Problems RoadSegmentGeometrySelfIntersects() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentGeometrySelfIntersects))), _requestedChanges);

        public Problems FakeRoadNodeConnectedSegmentsDoNotDiffer(RoadSegmentId segment1, RoadSegmentId segment2)
        {
            return new Problems(
                _problems.Add(
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

        public Problems RoadSegmentLaneAttributeFromPositionNotEqualToZero(AttributeId attributeId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentLaneAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentLaneAttributesNotAdjacent(AttributeId attributeId1, AttributeId attributeId2)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentLaneAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentLaneAttributeToPositionNotEqualToLength(AttributeId attributeId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentLaneAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentWidthAttributeFromPositionNotEqualToZero(AttributeId attributeId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentWidthAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentWidthAttributesNotAdjacent(AttributeId attributeId1, AttributeId attributeId2)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentWidthAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentWidthAttributeToPositionNotEqualToLength(AttributeId attributeId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentWidthAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(AttributeId attributeId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentSurfaceAttributesNotAdjacent(AttributeId attributeId1, AttributeId attributeId2)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentSurfaceAttributeToPositionNotEqualToLength(AttributeId attributeId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems RoadSegmentMissing(RoadSegmentId segmentId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentMissing),
                        new ProblemParameter("SegmentId", segmentId.ToInt32().ToString()))),
                _requestedChanges);
        }

        public Problems UpperRoadSegmentMissing()
        {
            return new Problems(
                _problems.Add(new Error(nameof(UpperRoadSegmentMissing))),
                _requestedChanges);
        }

        public Problems LowerRoadSegmentMissing()
        {
            return new Problems(
                _problems.Add(new Error(nameof(LowerRoadSegmentMissing))),
                _requestedChanges);
        }

        public Problems UpperAndLowerRoadSegmentDoNotIntersect()
        {
            return new Problems(
                _problems.Add(new Error(nameof(UpperAndLowerRoadSegmentDoNotIntersect))),
                _requestedChanges);
        }

        public Problems RoadSegmentPointMeasureValueOutOfRange(double pointX, double pointY, double measure, double measureLowerBoundary, double measureUpperBoundary)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentPointMeasureValueOutOfRange),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("MeasureLowerBoundary", measureLowerBoundary.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("MeasureUpperBoundary", measureUpperBoundary.ToString(CultureInfo.InvariantCulture)))),
                _requestedChanges);
        }

        public Problems RoadSegmentStartPointMeasureValueNotEqualToZero(double pointX, double pointY, double measure)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentStartPointMeasureValueNotEqualToZero),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)))),
                _requestedChanges);
        }

        public Problems RoadSegmentEndPointMeasureValueNotEqualToLength(double pointX, double pointY, double measure, double length)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentEndPointMeasureValueNotEqualToLength),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))),
                _requestedChanges);
        }

        public Problems RoadSegmentPointMeasureValueDoesNotIncrease(double pointX, double pointY, double measure, double previousMeasure)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentPointMeasureValueDoesNotIncrease),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PreviousMeasure", previousMeasure.ToString(CultureInfo.InvariantCulture)))),
                _requestedChanges);
        }
    }
}
