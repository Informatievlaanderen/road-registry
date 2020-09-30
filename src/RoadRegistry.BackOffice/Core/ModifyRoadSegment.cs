namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class ModifyRoadSegment : IRequestedChange
    {
        public ModifyRoadSegment(
            RoadSegmentId id,
            RoadNodeId startNodeId,
            RoadNodeId? temporaryStartNodeId,
            RoadNodeId endNodeId,
            RoadNodeId? temporaryEndNodeId,
            MultiLineString geometry,
            OrganizationId maintenanceAuthority,
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
            StartNodeId = startNodeId;
            TemporaryStartNodeId = temporaryStartNodeId;
            EndNodeId = endNodeId;
            TemporaryEndNodeId = temporaryEndNodeId;
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            MaintenanceAuthority = maintenanceAuthority;
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
        public RoadNodeId StartNodeId { get; }
        public RoadNodeId? TemporaryStartNodeId { get; }
        public RoadNodeId EndNodeId { get; }
        public RoadNodeId? TemporaryEndNodeId { get; }
        public MultiLineString Geometry { get; }
        public OrganizationId MaintenanceAuthority { get; }
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

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: We need a before and after verify because
            // in the before we want to make sure we're dealing with an existing segment

            var problems = Problems.None;

            if (!context.View.Segments.ContainsKey(Id))
            {
                problems = problems.RoadSegmentNotFound();
            }
            else
            {
                if (Math.Abs(Geometry.Length) <= context.Tolerance)
                {
                    problems = problems.RoadSegmentGeometryLengthIsZero();
                }

                var byOtherSegment =
                    context.View.Segments.Values.FirstOrDefault(segment =>
                        segment.Id != Id &&
                        segment.Geometry.EqualsExact(Geometry));
                if (byOtherSegment != null)
                {
                    problems = problems.RoadSegmentGeometryTaken(
                        context.Translator.TranslateToTemporaryOrId(byOtherSegment.Id)
                    );
                }

                var line = Geometry.Geometries
                    .OfType<LineString>()
                    .Single();
                if (!context.View.Nodes.TryGetValue(StartNodeId, out var startNode))
                {
                    problems = problems.RoadSegmentStartNodeMissing();
                }
                else
                {
                    if (line.StartPoint != null && !line.StartPoint.EqualsExact(startNode.Geometry))
                    {
                        problems = problems.RoadSegmentStartPointDoesNotMatchNodeGeometry();
                    }
                }

                if (!context.View.Nodes.TryGetValue(EndNodeId, out var endNode))
                {
                    problems = problems.RoadSegmentEndNodeMissing();
                }
                else
                {
                    if (line.EndPoint != null && !line.EndPoint.EqualsExact(endNode.Geometry))
                    {
                        problems = problems.RoadSegmentEndPointDoesNotMatchNodeGeometry();
                    }
                }

                if (line.SelfOverlaps())
                {
                    problems = problems.RoadSegmentGeometrySelfOverlaps();
                }
                else if (line.SelfIntersects())
                {
                    problems = problems.RoadSegmentGeometrySelfIntersects();
                }

                if (line.NumPoints > 0)
                {
                    var previousPointMeasure = 0.0;
                    for (var index = 0; index < line.CoordinateSequence.Count; index++)
                    {
                        var measure = line.CoordinateSequence.GetOrdinate(index, Ordinate.M);
                        var x = line.CoordinateSequence.GetX(index);
                        var y = line.CoordinateSequence.GetY(index);
                        if (index == 0 && Math.Abs(measure) > context.Tolerance)
                        {
                            problems =
                                problems.RoadSegmentStartPointMeasureValueNotEqualToZero(x, y, measure);
                        }
                        else if (index == line.CoordinateSequence.Count - 1 &&
                                 Math.Abs(measure - line.Length) > context.Tolerance)
                        {
                            problems =
                                problems.RoadSegmentEndPointMeasureValueNotEqualToLength(x, y, measure, line.Length);
                        }
                        else if (measure < 0.0 || measure > line.Length)
                        {
                            problems =
                                problems.RoadSegmentPointMeasureValueOutOfRange(x, y, measure, 0.0, line.Length);
                        }
                        else
                        {
                            if (index != 0 && Math.Sign(measure - previousPointMeasure) <= 0)
                            {
                                problems =
                                    problems.RoadSegmentPointMeasureValueDoesNotIncrease(x, y, measure,
                                        previousPointMeasure);
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
                                problems.RoadSegmentLaneAttributeFromPositionNotEqualToZero(
                                    lane.TemporaryId,
                                    lane.From);
                        }
                    }
                    else
                    {
                        if (lane.From != previousLane.To)
                        {
                            problems =
                                problems.RoadSegmentLaneAttributesNotAdjacent(
                                    previousLane.TemporaryId,
                                    previousLane.To,
                                    lane.TemporaryId,
                                    lane.From);
                        }
                    }

                    previousLane = lane;
                }

                if (previousLane != null)
                {
                    if (Math.Abs(previousLane.To.ToDouble() - line.Length) > context.Tolerance)
                    {
                        problems = problems.RoadSegmentLaneAttributeToPositionNotEqualToLength(
                            previousLane.TemporaryId,
                            previousLane.To,
                            line.Length);
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
                                problems.RoadSegmentWidthAttributeFromPositionNotEqualToZero(
                                    width.TemporaryId,
                                    width.From);
                        }
                    }
                    else
                    {
                        if (width.From != previousWidth.To)
                        {
                            problems =
                                problems.RoadSegmentWidthAttributesNotAdjacent(
                                    previousWidth.TemporaryId,
                                    previousWidth.To,
                                    width.TemporaryId,
                                    width.From);
                        }
                    }

                    previousWidth = width;
                }

                if (previousWidth != null)
                {
                    if (Math.Abs(previousWidth.To.ToDouble() - line.Length) > context.Tolerance)
                    {
                        problems = problems.RoadSegmentWidthAttributeToPositionNotEqualToLength(
                            previousWidth.TemporaryId,
                            previousWidth.To,
                            line.Length);
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
                                problems.RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(
                                    surface.TemporaryId,
                                    surface.From);
                        }
                    }
                    else
                    {
                        if (surface.From != previousSurface.To)
                        {
                            problems =
                                problems.RoadSegmentSurfaceAttributesNotAdjacent(
                                    previousSurface.TemporaryId,
                                    previousSurface.To,
                                    surface.TemporaryId,
                                    surface.From);
                        }
                    }

                    previousSurface = surface;
                }

                if (previousSurface != null)
                {
                    if (Math.Abs(previousSurface.To.ToDouble() - line.Length) > context.Tolerance)
                    {
                        problems = problems.RoadSegmentSurfaceAttributeToPositionNotEqualToLength(
                            previousSurface.TemporaryId, previousSurface.To, line.Length);
                    }
                }
            }

            if (problems.OfType<Error>().Any())
            {
                return new RejectedChange(this, problems);
            }

            return new AcceptedChange(this, problems);
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentModified = new Messages.RoadSegmentModified
            {
                Id = Id,
                StartNodeId = StartNodeId,
                EndNodeId = EndNodeId,
                Geometry = GeometryTranslator.Translate(Geometry),
                MaintenanceAuthority = new Messages.MaintenanceAuthority
                {
                    Code = MaintenanceAuthority
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

            message.ModifyRoadSegment = new Messages.ModifyRoadSegment
            {
                Id = Id,
                StartNodeId = TemporaryStartNodeId ?? StartNodeId,
                EndNodeId = TemporaryEndNodeId ?? EndNodeId,
                Geometry = GeometryTranslator.Translate(Geometry),
                MaintenanceAuthority = MaintenanceAuthority,
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
