namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class AddRoadSegment : IRequestedChange
    {
        public AddRoadSegment(
            RoadSegmentId id,
            RoadSegmentId temporaryId,
            RoadNodeId startNodeId,
            RoadNodeId? temporaryStartNodeId,
            RoadNodeId endNodeId,
            RoadNodeId? temporaryEndNodeId,
            MultiLineString geometry,
            OrganizationId maintenanceAuthorityId,
            OrganizationName? maintenanceAuthorityName,
            RoadSegmentGeometryDrawMethod geometryDrawMethod,
            RoadSegmentMorphology morphology,
            RoadSegmentStatus status,
            RoadSegmentCategory category,
            RoadSegmentAccessRestriction accessRestriction,
            CrabStreetnameId? leftSideStreetNameId,
            CrabStreetnameId? rightSideStreetNameId,
            IReadOnlyList<RoadSegmentLaneAttribute> lanes,
            IReadOnlyList<RoadSegmentWidthAttribute> widths,
            IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces)
        {
            Id = id;
            TemporaryId = temporaryId;
            StartNodeId = startNodeId;
            TemporaryStartNodeId = temporaryStartNodeId;
            EndNodeId = endNodeId;
            TemporaryEndNodeId = temporaryEndNodeId;
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            MaintenanceAuthorityId = maintenanceAuthorityId;
            MaintenanceAuthorityName = maintenanceAuthorityName;
            GeometryDrawMethod = geometryDrawMethod ?? throw new ArgumentNullException(nameof(geometryDrawMethod));
            Morphology = morphology ?? throw new ArgumentNullException(nameof(morphology));
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            AccessRestriction = accessRestriction ?? throw new ArgumentNullException(nameof(accessRestriction));
            LeftSideStreetNameId = leftSideStreetNameId;
            RightSideStreetNameId = rightSideStreetNameId;
            Lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
            Widths = widths ?? throw new ArgumentNullException(nameof(widths));
            Surfaces = surfaces ?? throw new ArgumentNullException(nameof(surfaces));
        }

        public RoadSegmentId Id { get; }
        public RoadSegmentId TemporaryId { get; }
        public RoadNodeId StartNodeId { get; }
        public RoadNodeId? TemporaryStartNodeId { get; }
        public RoadNodeId EndNodeId { get; }
        public RoadNodeId? TemporaryEndNodeId { get; }
        public MultiLineString Geometry { get; }
        public OrganizationId MaintenanceAuthorityId { get; }
        public OrganizationName? MaintenanceAuthorityName { get; }
        public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
        public RoadSegmentMorphology Morphology { get; }
        public RoadSegmentStatus Status { get; }
        public RoadSegmentCategory Category { get; }
        public RoadSegmentAccessRestriction AccessRestriction { get; }
        public CrabStreetnameId? LeftSideStreetNameId { get; }
        public CrabStreetnameId? RightSideStreetNameId { get; }
        public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
        public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }
        public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (Math.Abs(Geometry.Length) <= context.Tolerances.GeometryTolerance)
            {
                problems = problems.Add(new RoadSegmentGeometryLengthIsZero());
            }

            var line = Geometry.Geometries
                .OfType<LineString>()
                .Single();

            if (line.SelfOverlaps())
            {
                problems = problems.Add(new RoadSegmentGeometrySelfOverlaps());
            }
            else if (line.SelfIntersects())
            {
                problems = problems.Add(new RoadSegmentGeometrySelfIntersects());
            }

            if (line.NumPoints > 0)
            {
                var previousPointMeasure = 0.0;
                for (var index = 0; index < line.CoordinateSequence.Count; index++)
                {
                    var measure = line.CoordinateSequence.GetOrdinate(index, Ordinate.M);
                    var x = line.CoordinateSequence.GetX(index);
                    var y = line.CoordinateSequence.GetY(index);
                    if (index == 0 && Math.Abs(measure) > context.Tolerances.MeasurementTolerance)
                    {
                        problems =
                            problems.Add(new RoadSegmentStartPointMeasureValueNotEqualToZero(x, y, measure));
                    }
                    else if (index == line.CoordinateSequence.Count - 1 && Math.Abs(measure - line.Length) > context.Tolerances.MeasurementTolerance)
                    {
                        problems =
                            problems.Add(new RoadSegmentEndPointMeasureValueNotEqualToLength(x, y, measure, line.Length));
                    }
                    else if (measure < 0.0 || measure > line.Length)
                    {
                        problems =
                            problems.Add(new RoadSegmentPointMeasureValueOutOfRange(x, y, measure, 0.0, line.Length));
                    }
                    else
                    {
                        if (index != 0 && Math.Sign(measure - previousPointMeasure) <= 0)
                        {
                            problems =
                                problems.Add(new RoadSegmentPointMeasureValueDoesNotIncrease(x, y, measure, previousPointMeasure));
                        }
                        else
                        {
                            previousPointMeasure = measure;
                        }
                    }
                }
            }

            RoadSegmentLaneAttribute previousLane = null;
            foreach (var lane in Lanes)
            {
                if (previousLane == null)
                {
                    if (lane.From != RoadSegmentPosition.Zero)
                    {
                        problems =
                            problems.Add(new RoadSegmentLaneAttributeFromPositionNotEqualToZero(
                                lane.TemporaryId,
                                lane.From));
                    }
                }
                else
                {
                    if (lane.From != previousLane.To)
                    {
                        problems =
                            problems.Add(new RoadSegmentLaneAttributesNotAdjacent(
                                previousLane.TemporaryId,
                                previousLane.To,
                                lane.TemporaryId,
                                lane.From));
                    }

                    if (lane.From == lane.To)
                    {
                        problems =
                            problems.Add(new RoadSegmentLaneAttributeHasLengthOfZero(
                                lane.TemporaryId,
                                lane.From,
                                lane.To));
                    }
                }

                previousLane = lane;
            }

            if (previousLane != null)
            {
                if (Math.Abs(previousLane.To.ToDouble() - line.Length) > context.Tolerances.DynamicRoadSegmentAttributePositionTolerance)
                {
                    problems = problems.Add(new RoadSegmentLaneAttributeToPositionNotEqualToLength(
                        previousLane.TemporaryId,
                        previousLane.To,
                        line.Length));
                }
            }

            RoadSegmentWidthAttribute previousWidth = null;
            foreach (var width in Widths)
            {
                if (previousWidth == null)
                {
                    if (width.From != RoadSegmentPosition.Zero)
                    {
                        problems =
                            problems.Add(new RoadSegmentWidthAttributeFromPositionNotEqualToZero(
                                width.TemporaryId,
                                width.From));
                    }
                }
                else
                {
                    if (width.From != previousWidth.To)
                    {
                        problems =
                            problems.Add(new RoadSegmentWidthAttributesNotAdjacent(
                                previousWidth.TemporaryId,
                                previousWidth.To,
                                width.TemporaryId,
                                width.From));
                    }

                    if (width.From == width.To)
                    {
                        problems =
                            problems.Add(new RoadSegmentWidthAttributeHasLengthOfZero(
                                width.TemporaryId,
                                width.From,
                                width.To));
                    }
                }

                previousWidth = width;
            }

            if (previousWidth != null)
            {
                if (Math.Abs(previousWidth.To.ToDouble() - line.Length) > context.Tolerances.DynamicRoadSegmentAttributePositionTolerance)
                {
                    problems = problems.Add(new RoadSegmentWidthAttributeToPositionNotEqualToLength(
                        previousWidth.TemporaryId,
                        previousWidth.To,
                        line.Length));
                }
            }

            RoadSegmentSurfaceAttribute previousSurface = null;
            foreach (var surface in Surfaces)
            {
                if (previousSurface == null)
                {
                    if (surface.From != RoadSegmentPosition.Zero)
                    {
                        problems =
                            problems.Add(new RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(
                                surface.TemporaryId,
                                surface.From));
                    }
                }
                else
                {
                    if (surface.From != previousSurface.To)
                    {
                        problems =
                            problems.Add(new RoadSegmentSurfaceAttributesNotAdjacent(
                                previousSurface.TemporaryId,
                                previousSurface.To,
                                surface.TemporaryId,
                                surface.From));
                    }

                    if (surface.From == surface.To)
                    {
                        problems =
                            problems.Add(new RoadSegmentSurfaceAttributeHasLengthOfZero(
                                surface.TemporaryId,
                                surface.From,
                                surface.To));
                    }
                }

                previousSurface = surface;
            }

            if (previousSurface != null)
            {
                if (Math.Abs(previousSurface.To.ToDouble() - line.Length) > context.Tolerances.DynamicRoadSegmentAttributePositionTolerance)
                {
                    problems = problems.Add(new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(previousSurface.TemporaryId, previousSurface.To, line.Length));
                }
            }

            return problems;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var problems = Problems.None;

            var byOtherSegment =
                context.AfterView.Segments.Values.FirstOrDefault(segment =>
                    segment.Id != Id &&
                    segment.Geometry.EqualsWithinTolerance(Geometry, context.Tolerances.GeometryTolerance));
            if (byOtherSegment != null)
            {
                problems = problems.Add(new RoadSegmentGeometryTaken(
                    context.Translator.TranslateToTemporaryOrId(byOtherSegment.Id)
                ));
            }

            var line = Geometry.Geometries
                .OfType<LineString>()
                .Single();
            if (!context.AfterView.View.Nodes.TryGetValue(StartNodeId, out var startNode))
            {
                problems = problems.Add(new RoadSegmentStartNodeMissing());
            }
            else
            {
                problems = problems.AddRange(startNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
                if (line.StartPoint != null && !line.StartPoint.EqualsWithinTolerance(startNode.Geometry, context.Tolerances.GeometryTolerance))
                {
                    problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry());
                }
            }

            if (!context.AfterView.View.Nodes.TryGetValue(EndNodeId, out var endNode))
            {
                problems = problems.Add(new RoadSegmentEndNodeMissing());
            }
            else
            {
                problems = problems.AddRange(endNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
                if (line.EndPoint != null && !line.EndPoint.EqualsWithinTolerance(endNode.Geometry, context.Tolerances.GeometryTolerance))
                {
                    problems = problems.Add(new RoadSegmentEndPointDoesNotMatchNodeGeometry());
                }
            }

            if (!problems.Any())
            {
                var intersectingSegments = context.AfterView.View.CreateScopedView(Geometry.EnvelopeInternal)
                    .Segments.Where(pair => pair.Key != Id && pair.Value.Geometry.Intersects(Geometry));
                var intersectingSegmentsWithoutJunction = intersectingSegments.Where(intersectingSegment =>
                    !context.AfterView.GradeSeparatedJunctions.Any(junction =>
                        (junction.Value.LowerSegment == Id && junction.Value.UpperSegment == intersectingSegment.Key) ||
                        (junction.Value.LowerSegment == intersectingSegment.Key && junction.Value.UpperSegment == Id)));

                var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions =
                    intersectingSegmentsWithoutJunction.Select(i =>
                        new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                            context.Translator.TranslateToTemporaryOrId(Id),
                            context.Translator.TranslateToTemporaryOrId(i.Key)));
                problems = problems.AddRange(intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions);
            }

            return problems;
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentAdded = new Messages.RoadSegmentAdded
            {
                Id = Id,
                TemporaryId = TemporaryId,
                StartNodeId = StartNodeId,
                EndNodeId = EndNodeId,
                Geometry = GeometryTranslator.Translate(Geometry),
                MaintenanceAuthority = new Messages.MaintenanceAuthority
                {
                    Code = MaintenanceAuthorityId,
                    Name = MaintenanceAuthorityName ?? ""
                },
                GeometryDrawMethod = GeometryDrawMethod,
                Morphology = Morphology,
                Status = Status,
                Category = Category,
                AccessRestriction = AccessRestriction,
                LeftSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = LeftSideStreetNameId.GetValueOrDefault()
                },
                RightSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = RightSideStreetNameId.GetValueOrDefault()
                },
                Lanes = Lanes
                    .Select(item => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = 1,
                        Count = item.Count,
                        Direction = item.Direction,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Widths = Widths
                    .Select(item => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = 1,
                        Width = item.Width,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Surfaces = Surfaces
                    .Select(item => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = item.Id,
                        AsOfGeometryVersion = 1,
                        Type = item.Type,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray()
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegment = new Messages.AddRoadSegment
            {
                TemporaryId = TemporaryId,
                StartNodeId = TemporaryStartNodeId ?? StartNodeId,
                EndNodeId = TemporaryEndNodeId ?? EndNodeId,
                Geometry = GeometryTranslator.Translate(Geometry),
                MaintenanceAuthority = MaintenanceAuthorityId,
                GeometryDrawMethod = GeometryDrawMethod,
                Morphology = Morphology,
                Status = Status,
                Category = Category,
                AccessRestriction = AccessRestriction,
                LeftSideStreetNameId = LeftSideStreetNameId.GetValueOrDefault(),
                RightSideStreetNameId = RightSideStreetNameId.GetValueOrDefault(),
                Lanes = Lanes
                    .Select(item => new Messages.RequestedRoadSegmentLaneAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Count = item.Count,
                        Direction = item.Direction,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Widths = Widths
                    .Select(item => new Messages.RequestedRoadSegmentWidthAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Width = item.Width,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Surfaces = Surfaces
                    .Select(item => new Messages.RequestedRoadSegmentSurfaceAttribute
                    {
                        AttributeId = item.TemporaryId,
                        Type = item.Type,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray()
            };
        }
    }
}
