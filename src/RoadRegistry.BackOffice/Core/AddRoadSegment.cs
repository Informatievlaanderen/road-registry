namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;

public class AddRoadSegment : IRequestedChange, IHaveHash
{
    public const string EventName = "AddRoadSegment";

    public AddRoadSegment(
        RoadSegmentId id,
        RoadSegmentId temporaryId,
        RoadSegmentId? originalId,
        RoadSegmentId? permanentId,
        RoadNodeId startNodeId,
        RoadNodeId? temporaryStartNodeId,
        RoadNodeId endNodeId,
        RoadNodeId? temporaryEndNodeId,
        MultiLineString geometry,
        OrganizationId maintenanceAuthorityId,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        RoadSegmentCategory category,
        RoadSegmentAccessRestriction accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId,
        IReadOnlyList<RoadSegmentLaneAttribute> lanes,
        IReadOnlyList<RoadSegmentWidthAttribute> widths,
        IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces)
    {
        Id = id;
        TemporaryId = temporaryId;
        OriginalId = originalId;
        PermanentId = permanentId;
        StartNodeId = startNodeId;
        TemporaryStartNodeId = temporaryStartNodeId;
        EndNodeId = endNodeId;
        TemporaryEndNodeId = temporaryEndNodeId;
        Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        MaintenanceAuthorityId = maintenanceAuthorityId;
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
    public StreetNameLocalId? LeftSideStreetNameId { get; }
    public OrganizationId MaintenanceAuthorityId { get; }
    public RoadSegmentMorphology Morphology { get; }
    public StreetNameLocalId? RightSideStreetNameId { get; }
    public RoadNodeId StartNodeId { get; }
    public RoadSegmentStatus Status { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public RoadNodeId? TemporaryEndNodeId { get; }
    public RoadSegmentId TemporaryId { get; }
    public RoadSegmentId? OriginalId { get; }
    public RoadSegmentId? PermanentId { get; }
    public RoadNodeId? TemporaryStartNodeId { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings, AfterVerificationContext context)
    {
        var maintainer = context.Organizations.FindAsync(MaintenanceAuthorityId, CancellationToken.None).GetAwaiter().GetResult();

        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentAdded = new RoadSegmentAdded
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
                    Name = OrganizationName.FromValueWithFallback(maintainer?.Translation.Name)
                },
                GeometryDrawMethod = GeometryDrawMethod,
                Morphology = Morphology,
                Status = Status,
                Category = Category,
                AccessRestriction = AccessRestriction,
                LeftSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = LeftSideStreetNameId
                },
                RightSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = RightSideStreetNameId
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
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegment = new Messages.AddRoadSegment
        {
            TemporaryId = TemporaryId,
            OriginalId = OriginalId,
            PermanentId = PermanentId,
            StartNodeId = TemporaryStartNodeId ?? StartNodeId,
            EndNodeId = TemporaryEndNodeId ?? EndNodeId,
            Geometry = GeometryTranslator.Translate(Geometry),
            MaintenanceAuthority = MaintenanceAuthorityId,
            GeometryDrawMethod = GeometryDrawMethod,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            AccessRestriction = AccessRestriction,
            LeftSideStreetNameId = LeftSideStreetNameId,
            RightSideStreetNameId = RightSideStreetNameId,
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

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;
        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);

        var line = Geometry.GetSingleLineString();

        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);

            return VerifyAfterResult.WithAcceptedChanges(problems, warnings => TranslateTo(warnings, context));
        }

        var byOtherSegment =
            context.AfterView.Segments.Values.FirstOrDefault(segment =>
                segment.Id != Id &&
                segment.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
        if (byOtherSegment != null)
        {
            problems = problems.Add(new RoadSegmentGeometryTaken(
                context.Translator.TranslateToOriginalOrTemporaryOrId(byOtherSegment.Id)
            ));
        }

        if (!context.AfterView.View.Nodes.TryGetValue(StartNodeId, out var startNode))
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(startNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line.StartPoint != null && !line.StartPoint.IsReasonablyEqualTo(startNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!context.AfterView.View.Nodes.TryGetValue(EndNodeId, out var endNode))
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(endNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line.EndPoint != null && !line.EndPoint.IsReasonablyEqualTo(endNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentEndPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!problems.Any())
        {
            var intersectingSegments = context.AfterView.View.CreateScopedView(Geometry.EnvelopeInternal)
                .FindIntersectingRoadSegments(this);
            var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegments
                .Where(intersectingSegment =>
                    !context.AfterView.GradeSeparatedJunctions.Any(junction =>
                        (junction.Value.LowerSegment == Id && junction.Value.UpperSegment == intersectingSegment.Key) ||
                        (junction.Value.LowerSegment == intersectingSegment.Key && junction.Value.UpperSegment == Id)))
                .Select(i =>
                    new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                        originalIdOrId,
                        context.Translator.TranslateToOriginalOrTemporaryOrId(i.Key)));

            problems = problems.AddRange(intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions);
        }

        return VerifyAfterResult.WithAcceptedChanges(problems, warnings => TranslateTo(warnings, context));
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;
        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);

        var line = Geometry.GetSingleLineString();

        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);

            return problems;
        }

        problems += line.GetProblemsForRoadSegmentGeometry(originalIdOrId, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentLanes(Lanes, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentWidths(Widths, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentSurfaces(Surfaces, context.Tolerances);

        return problems;
    }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
