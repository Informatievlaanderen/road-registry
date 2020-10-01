namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;

    public class Problems : IReadOnlyCollection<Problem>
    {
        private readonly ImmutableList<Problem> _problems;

        public static readonly Problems None = new Problems(ImmutableList<Problem>.Empty);

        private Problems(ImmutableList<Problem> problems)
        {
            _problems = problems;
        }

        public IEnumerator<Problem> GetEnumerator() => _problems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _problems.Count;

        // Road Node Errors

        //public Errors RoadNodeIdTaken() => new Errors(_errors.Add(new Error(nameof(RoadNodeIdTaken))));

        public Problems RoadNodeGeometryTaken(RoadNodeId byOtherNode)
        {
            return new Problems(_problems.Add(
                new Error(
                    nameof(RoadNodeGeometryTaken),
                    new ProblemParameter(
                        "ByOtherNode",
                        byOtherNode.ToInt32().ToString())))
            );
        }

        public Problems RoadNodeNotFound()
        {
            return new Problems(_problems.Add(new Error(nameof(RoadNodeNotFound))));
        }

        public Problems RoadNodeTooClose(RoadSegmentId toOtherSegment)
        {
            return new Problems(_problems.Add(
                    new Error(
                        nameof(RoadNodeTooClose),
                        new ProblemParameter(
                            "ToOtherSegment",
                            toOtherSegment.ToInt32().ToString())))
            );
        }

        public Problems RoadNodeNotConnectedToAnySegment() => new Problems(_problems.Add(new Error(nameof(RoadNodeNotConnectedToAnySegment))));

        public Problems RoadNodeTypeMismatch(int connectedSegmentCount, RoadNodeType actualType, RoadNodeType[] expectedTypes)
        {
            if (expectedTypes == null)
                throw new ArgumentNullException(nameof(expectedTypes));
            if (expectedTypes.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(expectedTypes));

            var parameters = new List<ProblemParameter>
            {
                new ProblemParameter("ConnectedSegmentCount",
                    connectedSegmentCount.ToString(CultureInfo.InvariantCulture)),
                new ProblemParameter("Actual", actualType.ToString())
            };
            parameters.AddRange(expectedTypes.Select(type => new ProblemParameter("Expected", type.ToString())));

            return new Problems(_problems.Add(new Error(nameof(RoadNodeTypeMismatch), parameters.ToArray())));
        }

        public Problems FakeRoadNodeConnectedSegmentsDoNotDiffer(RoadSegmentId segment1, RoadSegmentId segment2)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(FakeRoadNodeConnectedSegmentsDoNotDiffer),
                        new ProblemParameter(
                            "SegmentId",
                            segment1.ToInt32().ToString()),
                        new ProblemParameter(
                            "SegmentId",
                            segment2.ToInt32().ToString())))
            );
        }

        // Road Segment Errors

        //public Errors RoadSegmentIdTaken() => new Errors(_errors.Add(new Error(nameof(RoadSegmentIdTaken))));

        public Problems RoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        {
            return new Problems(_problems.Add(
                    new Error(
                        nameof(RoadSegmentGeometryTaken),
                        new ProblemParameter(
                            "ByOtherSegment",
                            byOtherSegment.ToInt32().ToString()))));
        }

        public Problems RoadSegmentStartNodeMissing() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentStartNodeMissing))));

        public Problems RoadSegmentEndNodeMissing() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentEndNodeMissing))));

        public Problems RoadSegmentGeometryLengthIsZero() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentGeometryLengthIsZero))));

        public Problems RoadSegmentStartPointDoesNotMatchNodeGeometry() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentStartPointDoesNotMatchNodeGeometry))));

        public Problems RoadSegmentEndPointDoesNotMatchNodeGeometry() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentEndPointDoesNotMatchNodeGeometry))));

        public Problems RoadSegmentGeometrySelfOverlaps() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentGeometrySelfOverlaps))));

        public Problems RoadSegmentGeometrySelfIntersects() =>
            new Problems(_problems.Add(new Error(nameof(RoadSegmentGeometrySelfIntersects))));

        public Problems RoadSegmentMissing(RoadSegmentId segmentId)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentMissing),
                        new ProblemParameter("SegmentId", segmentId.ToInt32().ToString())))
            );
        }

        public Problems EuropeanRoadNumberNotFound(EuropeanRoadNumber number)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(EuropeanRoadNumberNotFound),
                        new ProblemParameter("Number", number.ToString())))
            );
        }

        public Problems NationalRoadNumberNotFound(NationalRoadNumber number)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(NationalRoadNumberNotFound),
                        new ProblemParameter("Number", number.ToString())))
            );
        }

        public Problems NumberedRoadNumberNotFound(NumberedRoadNumber number)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(NumberedRoadNumberNotFound),
                        new ProblemParameter("Number", number.ToString())))
            );
        }

        public Problems RoadSegmentNotFound()
        {
            return new Problems(_problems.Add(new Error(nameof(RoadSegmentNotFound))));
        }

        public Problems RoadSegmentLaneAttributeFromPositionNotEqualToZero(AttributeId attributeId,
            RoadSegmentPosition fromPosition)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentLaneAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                        new ProblemParameter("FromPosition", fromPosition.ToString()))));
        }

        public Problems RoadSegmentLaneAttributesNotAdjacent(AttributeId attributeId1, RoadSegmentPosition toPosition, AttributeId attributeId2, RoadSegmentPosition fromPosition)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentLaneAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("ToPosition", toPosition.ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()),
                        new ProblemParameter("FromPosition", fromPosition.ToString()))));
        }

        public Problems RoadSegmentLaneAttributeToPositionNotEqualToLength(AttributeId attributeId,
            RoadSegmentPosition toPosition, double length)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentLaneAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                        new ProblemParameter("ToPosition", toPosition.ToString()),
                        new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))));
        }

        public Problems RoadSegmentWidthAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentWidthAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                        new ProblemParameter("FromPosition", fromPosition.ToString()))));
        }

        public Problems RoadSegmentWidthAttributesNotAdjacent(AttributeId attributeId1, RoadSegmentPosition toPosition, AttributeId attributeId2, RoadSegmentPosition fromPosition)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentWidthAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("ToPosition", toPosition.ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()),
                        new ProblemParameter("FromPosition", fromPosition.ToString()))));
        }

        public Problems RoadSegmentWidthAttributeToPositionNotEqualToLength(AttributeId attributeId, RoadSegmentPosition toPosition, double length)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentWidthAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                        new ProblemParameter("ToPosition", toPosition.ToString()),
                        new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))));
        }

        public Problems RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                        new ProblemParameter("FromPosition", fromPosition.ToString()))));
        }

        public Problems RoadSegmentSurfaceAttributesNotAdjacent(AttributeId attributeId1, RoadSegmentPosition toPosition, AttributeId attributeId2, RoadSegmentPosition fromPosition)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("ToPosition", toPosition.ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()),
                        new ProblemParameter("FromPosition", fromPosition.ToString()))));
        }

        public Problems RoadSegmentSurfaceAttributeToPositionNotEqualToLength(AttributeId attributeId, RoadSegmentPosition toPosition, double length)
        {
            return new Problems(
                _problems.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                        new ProblemParameter("ToPosition", toPosition.ToString()),
                        new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))));
        }

        public Problems RoadSegmentPointMeasureValueOutOfRange(double pointX, double pointY, double measure, double measureLowerBoundary, double measureUpperBoundary)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentPointMeasureValueOutOfRange),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("MeasureLowerBoundary", measureLowerBoundary.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("MeasureUpperBoundary", measureUpperBoundary.ToString(CultureInfo.InvariantCulture)))));
        }

        public Problems RoadSegmentStartPointMeasureValueNotEqualToZero(double pointX, double pointY, double measure)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentStartPointMeasureValueNotEqualToZero),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)))));
        }

        public Problems RoadSegmentEndPointMeasureValueNotEqualToLength(double pointX, double pointY, double measure, double length)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentEndPointMeasureValueNotEqualToLength),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))));
        }

        public Problems RoadSegmentPointMeasureValueDoesNotIncrease(double pointX, double pointY, double measure, double previousMeasure)
        {
            return new Problems(
                _problems.Add(new Error(nameof(RoadSegmentPointMeasureValueDoesNotIncrease),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PreviousMeasure", previousMeasure.ToString(CultureInfo.InvariantCulture)))));
        }

        // Grade Separated Junction Errors


        public Problems GradeSeparatedJunctionNotFound()
        {
            return new Problems(_problems.Add(new Error(nameof(GradeSeparatedJunctionNotFound))));
        }

        public Problems UpperRoadSegmentMissing() => new Problems(_problems.Add(new Error(nameof(UpperRoadSegmentMissing))));
        public Problems LowerRoadSegmentMissing() => new Problems(_problems.Add(new Error(nameof(LowerRoadSegmentMissing))));
        public Problems UpperAndLowerRoadSegmentDoNotIntersect() => new Problems(_problems.Add(new Error(nameof(UpperAndLowerRoadSegmentDoNotIntersect))));
    }
}
