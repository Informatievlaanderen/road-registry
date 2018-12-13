namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;

    public class Errors : IReadOnlyCollection<Error>
    {
        private readonly ImmutableList<Error> _errors;

        public static readonly Errors None = new Errors(ImmutableList<Error>.Empty);

        private Errors(ImmutableList<Error> errors)
        {
            _errors = errors;
        }

        public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _errors.Count;

        // Road Node Errors

        //public Errors RoadNodeIdTaken() => new Errors(_errors.Add(new Error(nameof(RoadNodeIdTaken))));

        public Errors RoadNodeGeometryTaken(RoadNodeId byOtherNode)
        {
            return new Errors(_errors.Add(
                new Error(
                    nameof(RoadNodeGeometryTaken),
                    new ProblemParameter(
                        "ByOtherNode",
                        byOtherNode.ToInt32().ToString())))
            );
        }

//        public Errors RoadNodeTooClose(RoadNodeId toOtherNode)
//        {
//            return new Errors(_errors.Add(
//                    new Error(
//                        nameof(RoadNodeTooClose),
//                        new ProblemParameter(
//                            "ToOtherNode",
//                            toOtherNode.ToInt32().ToString())))
//            );
//        }

        public Errors RoadNodeNotConnectedToAnySegment() => new Errors(_errors.Add(new Error(nameof(RoadNodeNotConnectedToAnySegment))));

        public Errors RoadNodeTypeMismatch(params RoadNodeType[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (types.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(types));
            return new Errors(_errors.Add(
                new Error(
                    nameof(RoadNodeTypeMismatch),
                    Array.ConvertAll(types, type => new ProblemParameter("Expected", type.ToString()))))
            );
        }

        public Errors FakeRoadNodeConnectedSegmentsDoNotDiffer(RoadSegmentId segment1, RoadSegmentId segment2)
        {
            return new Errors(
                _errors.Add(
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

        public Errors RoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        {
            return new Errors(_errors.Add(
                    new Error(
                        nameof(RoadSegmentGeometryTaken),
                        new ProblemParameter(
                            "ByOtherSegment",
                            byOtherSegment.ToInt32().ToString()))));
        }

        public Errors RoadSegmentStartNodeMissing() =>
            new Errors(_errors.Add(new Error(nameof(RoadSegmentStartNodeMissing))));

        public Errors RoadSegmentEndNodeMissing() =>
            new Errors(_errors.Add(new Error(nameof(RoadSegmentEndNodeMissing))));

        public Errors RoadSegmentGeometryLengthIsZero() =>
            new Errors(_errors.Add(new Error(nameof(RoadSegmentGeometryLengthIsZero))));

        public Errors RoadSegmentStartPointDoesNotMatchNodeGeometry() =>
            new Errors(_errors.Add(new Error(nameof(RoadSegmentStartPointDoesNotMatchNodeGeometry))));

        public Errors RoadSegmentEndPointDoesNotMatchNodeGeometry() =>
            new Errors(_errors.Add(new Error(nameof(RoadSegmentEndPointDoesNotMatchNodeGeometry))));

        public Errors RoadSegmentGeometrySelfOverlaps() =>
            new Errors(_errors.Add(new Error(nameof(RoadSegmentGeometrySelfOverlaps))));

        public Errors RoadSegmentGeometrySelfIntersects() =>
            new Errors(_errors.Add(new Error(nameof(RoadSegmentGeometrySelfIntersects))));

        public Errors RoadSegmentMissing(RoadSegmentId segmentId)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentMissing),
                        new ProblemParameter("SegmentId", segmentId.ToInt32().ToString())))
            );
        }

        public Errors RoadSegmentLaneAttributeFromPositionNotEqualToZero(AttributeId attributeId)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentLaneAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))));
        }

        public Errors RoadSegmentLaneAttributesNotAdjacent(AttributeId attributeId1, AttributeId attributeId2)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentLaneAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()))));
        }

        public Errors RoadSegmentLaneAttributeToPositionNotEqualToLength(AttributeId attributeId)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentLaneAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))));
        }

        public Errors RoadSegmentWidthAttributeFromPositionNotEqualToZero(AttributeId attributeId)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentWidthAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))));
        }

        public Errors RoadSegmentWidthAttributesNotAdjacent(AttributeId attributeId1, AttributeId attributeId2)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentWidthAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()))));
        }

        public Errors RoadSegmentWidthAttributeToPositionNotEqualToLength(AttributeId attributeId)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentWidthAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))));
        }

        public Errors RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(AttributeId attributeId)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributeFromPositionNotEqualToZero),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))));
        }

        public Errors RoadSegmentSurfaceAttributesNotAdjacent(AttributeId attributeId1, AttributeId attributeId2)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributesNotAdjacent),
                        new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                        new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()))));
        }

        public Errors RoadSegmentSurfaceAttributeToPositionNotEqualToLength(AttributeId attributeId)
        {
            return new Errors(
                _errors.Add(
                    new Error(nameof(RoadSegmentSurfaceAttributeToPositionNotEqualToLength),
                        new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()))));
        }

        public Errors RoadSegmentPointMeasureValueOutOfRange(double pointX, double pointY, double measure, double measureLowerBoundary, double measureUpperBoundary)
        {
            return new Errors(
                _errors.Add(new Error(nameof(RoadSegmentPointMeasureValueOutOfRange),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("MeasureLowerBoundary", measureLowerBoundary.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("MeasureUpperBoundary", measureUpperBoundary.ToString(CultureInfo.InvariantCulture)))));
        }

        public Errors RoadSegmentStartPointMeasureValueNotEqualToZero(double pointX, double pointY, double measure)
        {
            return new Errors(
                _errors.Add(new Error(nameof(RoadSegmentStartPointMeasureValueNotEqualToZero),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)))));
        }

        public Errors RoadSegmentEndPointMeasureValueNotEqualToLength(double pointX, double pointY, double measure, double length)
        {
            return new Errors(
                _errors.Add(new Error(nameof(RoadSegmentEndPointMeasureValueNotEqualToLength),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))));
        }

        public Errors RoadSegmentPointMeasureValueDoesNotIncrease(double pointX, double pointY, double measure, double previousMeasure)
        {
            return new Errors(
                _errors.Add(new Error(nameof(RoadSegmentPointMeasureValueDoesNotIncrease),
                    new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
                    new ProblemParameter("PreviousMeasure", previousMeasure.ToString(CultureInfo.InvariantCulture)))));
        }

        // Grade Separated Junction Errors
        public Errors UpperRoadSegmentMissing() => new Errors(_errors.Add(new Error(nameof(UpperRoadSegmentMissing))));
        public Errors LowerRoadSegmentMissing() => new Errors(_errors.Add(new Error(nameof(LowerRoadSegmentMissing))));
        public Errors UpperAndLowerRoadSegmentDoNotIntersect() => new Errors(_errors.Add(new Error(nameof(UpperAndLowerRoadSegmentDoNotIntersect))));
    }
}
