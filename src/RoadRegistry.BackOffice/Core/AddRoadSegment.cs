namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using NetTopologySuite.Geometries;
using LineString = NetTopologySuite.Geometries.LineString;

public class AddRoadSegment : IRequestedChange, IHaveHash
{
    public const string EventName = "AddRoadSegment";

    public AddRoadSegment(
        RoadSegmentId id,
        RoadSegmentId temporaryId,
        RoadSegmentId? originalId,
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
        OriginalId = originalId;
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

    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public RoadNodeId EndNodeId { get; }
    public MultiLineString Geometry { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public RoadSegmentId Id { get; }
    public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
    public CrabStreetnameId? LeftSideStreetNameId { get; }
    public OrganizationId MaintenanceAuthorityId { get; }
    public OrganizationName? MaintenanceAuthorityName { get; }
    public RoadSegmentMorphology Morphology { get; }
    public CrabStreetnameId? RightSideStreetNameId { get; }
    public RoadNodeId StartNodeId { get; }
    public RoadSegmentStatus Status { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public RoadNodeId? TemporaryEndNodeId { get; }
    public RoadSegmentId TemporaryId { get; }
    public RoadSegmentId? OriginalId { get; }
    public RoadNodeId? TemporaryStartNodeId { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentAdded = new RoadSegmentAdded
        {
            Id = Id,
            Version = RoadSegmentVersion.Initial,
            TemporaryId = TemporaryId,
            OriginalId = OriginalId,
            StartNodeId = StartNodeId,
            EndNodeId = EndNodeId,
            Geometry = GeometryTranslator.Translate(Geometry),
            GeometryVersion = GeometryVersion.Initial,
            MaintenanceAuthority = new MaintenanceAuthority
            {
                Code = MaintenanceAuthorityId,
                Name = MaintenanceAuthorityName ?? string.Empty
            },
            GeometryDrawMethod = GeometryDrawMethod,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            AccessRestriction = AccessRestriction,
            LeftSide = new RoadSegmentSideAttributes
            {
                StreetNameId = LeftSideStreetNameId.GetValueOrDefault()
            },
            RightSide = new RoadSegmentSideAttributes
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
            OriginalId = OriginalId,
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
                .Select(item => new RequestedRoadSegmentLaneAttribute
                {
                    AttributeId = item.TemporaryId,
                    Count = item.Count,
                    Direction = item.Direction,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Widths = Widths
                .Select(item => new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = item.TemporaryId,
                    Width = item.Width,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Surfaces = Surfaces
                .Select(item => new RequestedRoadSegmentSurfaceAttribute
                {
                    AttributeId = item.TemporaryId,
                    Type = item.Type,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray()
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        var line = Geometry.Geometries
            .OfType<LineString>()
            .Single();

        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems.AddRange(line.GetProblemsForRoadSegmentOutlinedGeometry(context.Tolerances));

            return problems;
        }

        var byOtherSegment =
            context.AfterView.Segments.Values.FirstOrDefault(segment =>
                segment.Id != Id &&
                segment.Geometry.EqualsWithinTolerance(Geometry, context.Tolerances.GeometryTolerance));
        if (byOtherSegment != null)
            problems = problems.Add(new RoadSegmentGeometryTaken(
                context.Translator.TranslateToTemporaryOrId(byOtherSegment.Id)
            ));


        if (!context.AfterView.View.Nodes.TryGetValue(StartNodeId, out var startNode))
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing());
        }
        else
        {
            problems = problems.AddRange(startNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line.StartPoint != null && !line.StartPoint.EqualsWithinTolerance(startNode.Geometry, context.Tolerances.GeometryTolerance)) problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry());
        }

        if (!context.AfterView.View.Nodes.TryGetValue(EndNodeId, out var endNode))
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing());
        }
        else
        {
            problems = problems.AddRange(endNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line.EndPoint != null && !line.EndPoint.EqualsWithinTolerance(endNode.Geometry, context.Tolerances.GeometryTolerance)) problems = problems.Add(new RoadSegmentEndPointDoesNotMatchNodeGeometry());
        }

        if (!problems.Any())
        {
            var intersectingSegments = context.AfterView.View.CreateScopedView(Geometry.EnvelopeInternal).FindIntersectingRoadSegments(this);
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

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        var line = Geometry.Geometries
            .OfType<LineString>()
            .Single();

        var problems = line.GetProblemsForRoadSegmentGeometry(context.Tolerances);

        RoadSegmentLaneAttribute previousLane = null;
        foreach (var lane in Lanes)
        {
            if (previousLane == null)
            {
                if (lane.From != RoadSegmentPosition.Zero)
                    problems =
                        problems.Add(new RoadSegmentLaneAttributeFromPositionNotEqualToZero(
                            lane.TemporaryId,
                            lane.From));
            }
            else
            {
                if (lane.From != previousLane.To)
                    problems =
                        problems.Add(new RoadSegmentLaneAttributesNotAdjacent(
                            previousLane.TemporaryId,
                            previousLane.To,
                            lane.TemporaryId,
                            lane.From));

                if (lane.From == lane.To)
                    problems =
                        problems.Add(new RoadSegmentLaneAttributeHasLengthOfZero(
                            lane.TemporaryId,
                            lane.From,
                            lane.To));
            }

            previousLane = lane;
        }

        if (previousLane != null
            && !previousLane.To.ToDouble().EqualsWithTolerance(line.Length, context.Tolerances.DynamicRoadSegmentAttributePositionTolerance))
            problems = problems.Add(new RoadSegmentLaneAttributeToPositionNotEqualToLength(
                previousLane.TemporaryId,
                previousLane.To,
                line.Length));

        RoadSegmentWidthAttribute previousWidth = null;
        foreach (var width in Widths)
        {
            if (previousWidth == null)
            {
                if (width.From != RoadSegmentPosition.Zero)
                    problems =
                        problems.Add(new RoadSegmentWidthAttributeFromPositionNotEqualToZero(
                            width.TemporaryId,
                            width.From));
            }
            else
            {
                if (width.From != previousWidth.To)
                    problems =
                        problems.Add(new RoadSegmentWidthAttributesNotAdjacent(
                            previousWidth.TemporaryId,
                            previousWidth.To,
                            width.TemporaryId,
                            width.From));

                if (width.From == width.To)
                    problems =
                        problems.Add(new RoadSegmentWidthAttributeHasLengthOfZero(
                            width.TemporaryId,
                            width.From,
                            width.To));
            }

            previousWidth = width;
        }

        if (previousWidth != null
            && !previousWidth.To.ToDouble().EqualsWithTolerance(line.Length, context.Tolerances.DynamicRoadSegmentAttributePositionTolerance))
            problems = problems.Add(new RoadSegmentWidthAttributeToPositionNotEqualToLength(
                previousWidth.TemporaryId,
                previousWidth.To,
                line.Length));

        RoadSegmentSurfaceAttribute previousSurface = null;
        foreach (var surface in Surfaces)
        {
            if (previousSurface == null)
            {
                if (surface.From != RoadSegmentPosition.Zero)
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(
                            surface.TemporaryId,
                            surface.From));
            }
            else
            {
                if (surface.From != previousSurface.To)
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributesNotAdjacent(
                            previousSurface.TemporaryId,
                            previousSurface.To,
                            surface.TemporaryId,
                            surface.From));

                if (surface.From == surface.To)
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributeHasLengthOfZero(
                            surface.TemporaryId,
                            surface.From,
                            surface.To));
            }

            previousSurface = surface;
        }

        if (previousSurface != null
            && !previousSurface.To.ToDouble().EqualsWithTolerance(line.Length, context.Tolerances.DynamicRoadSegmentAttributePositionTolerance))
            problems = problems.Add(new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(previousSurface.TemporaryId, previousSurface.To, line.Length));

        return problems;
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
