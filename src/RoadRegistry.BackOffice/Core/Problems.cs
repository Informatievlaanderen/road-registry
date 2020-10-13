namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;

    public class Problems : IReadOnlyCollection<Problem>
    {
        private readonly ImmutableList<Problem> _problems;

        public static readonly Problems None = new Problems(ImmutableList<Problem>.Empty);

        public static Problems Single(Problem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));

            return None.Add(problem);
        }

        public static Problems Many(params Problem[] problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            return None.AddRange(problems);
        }

        public static Problems Many(IEnumerable<Problem> problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));

            return None.AddRange(problems);
        }

        private Problems(ImmutableList<Problem> problems)
        {
            _problems = problems;
        }

        public IEnumerator<Problem> GetEnumerator() => _problems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _problems.Count;

        public Problems Add(Problem problem)
        {
            if (problem == null) throw new ArgumentNullException(nameof(problem));
            return new Problems(_problems.Add(problem));
        }

        public Problems AddRange(IEnumerable<Problem> problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));
            return new Problems(_problems.AddRange(problems));
        }

        public Problems AddRange(Problems problems)
        {
            if (problems == null) throw new ArgumentNullException(nameof(problems));
            return new Problems(_problems.AddRange(problems));
        }

        public static Problems operator +(Problems left, Problem right)
            => left.Add(right);

        public static Problems operator +(Problems left, IEnumerable<Problem> right)
            => left.AddRange(right);

        public static Problems operator +(Problems left, Problems right)
            => left.AddRange(right);

        // Road Segment Errors

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
    }
}
