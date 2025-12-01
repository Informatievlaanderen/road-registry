namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;
using Messages;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public class ModifyRoadSegment : IRequestedChange, IHaveHash
{
    public const string EventName = "ModifyRoadSegment";

    public ModifyRoadSegment(
        RoadSegmentId id,
        RoadSegmentId? originalId,
        RoadSegmentVersion version,
        RoadNodeId? startNodeId,
        RoadNodeId? temporaryStartNodeId,
        RoadNodeId? endNodeId,
        RoadNodeId? temporaryEndNodeId,
        MultiLineString? geometry,
        GeometryVersion? geometryVersion,
        OrganizationId? maintenanceAuthorityId,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentMorphology? morphology,
        RoadSegmentStatus? status,
        RoadSegmentCategory? category,
        RoadSegmentAccessRestriction? accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId,
        IReadOnlyList<RoadSegmentLaneAttribute>? lanes,
        IReadOnlyList<RoadSegmentWidthAttribute>? widths,
        IReadOnlyList<RoadSegmentSurfaceAttribute>? surfaces,
        bool convertedFromOutlined,
        bool? categoryModified)
    {
        Id = id;
        OriginalId = originalId;
        Version = version;
        StartNodeId = startNodeId;
        TemporaryStartNodeId = temporaryStartNodeId;
        EndNodeId = endNodeId;
        TemporaryEndNodeId = temporaryEndNodeId;
        Geometry = geometry;
        GeometryVersion = geometryVersion;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        GeometryDrawMethod = geometryDrawMethod.ThrowIfNull();
        Morphology = morphology;
        Status = status;
        Category = category;
        AccessRestriction = accessRestriction;
        LeftSideStreetNameId = leftSideStreetNameId;
        RightSideStreetNameId = rightSideStreetNameId;
        Lanes = lanes;
        Widths = widths;
        Surfaces = surfaces;
        ConvertedFromOutlined = convertedFromOutlined;
        CategoryModified = categoryModified;
    }

    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public RoadSegmentVersion Version { get; }
    public GeometryVersion? GeometryVersion { get; }
    public MultiLineString? Geometry { get; }
    public RoadNodeId? StartNodeId { get; }
    public RoadNodeId? EndNodeId { get; }
    public RoadSegmentAccessRestriction? AccessRestriction { get; }
    public RoadSegmentCategory? Category { get; }
    public OrganizationId? MaintenanceAuthorityId { get; }
    public RoadSegmentMorphology? Morphology { get; }
    public RoadSegmentStatus? Status { get; }
    public RoadSegmentId? OriginalId { get; }
    public StreetNameLocalId? LeftSideStreetNameId { get; }
    public StreetNameLocalId? RightSideStreetNameId { get; }
    public IReadOnlyList<RoadSegmentLaneAttribute>? Lanes { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute>? Surfaces { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute>? Widths { get; }
    public RoadNodeId? TemporaryEndNodeId { get; }
    public RoadNodeId? TemporaryStartNodeId { get; }
    public bool ConvertedFromOutlined { get; }
    public bool? CategoryModified { get; }
    private RoadSegmentCategory? _correctedCategory;

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;
        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);

        if (!context.BeforeView.Segments.TryGetValue(Id, out var beforeSegment) && !ConvertedFromOutlined)
        {
            problems = problems.Add(new RoadSegmentNotFound(originalIdOrId));
        }

        var line = Geometry?.GetSingleLineString();

        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            if (line is not null)
            {
                problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId);
            }

            return problems;
        }

        if (line is not null)
        {
            problems += line.ValidateRoadSegmentGeometry(originalIdOrId);
        }

        if (beforeSegment is not null && CategoryModified is not null && !CategoryModified.Value && RoadSegmentCategory.IsUpgraded(beforeSegment.AttributeHash.Category))
        {
            _correctedCategory = beforeSegment.AttributeHash.Category;
            problems += new RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion(originalIdOrId);
        }

        return problems;
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;
        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);
        var afterSegment = context.AfterView.Segments[Id];

        var line = Geometry?.GetSingleLineString();

        if (GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            if (line is not null)
            {
                problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId);
            }

            return VerifyAfterResult.WithAcceptedChanges(problems, warnings => BuildAcceptedChanges(warnings, context));
        }

        var startNodeId = StartNodeId ?? afterSegment.Start;
        var endNodeId = EndNodeId ?? afterSegment.End;

        if (Geometry is not null)
        {
            var byOtherSegment =
                context.AfterView.Segments.Values.FirstOrDefault(segment =>
                    segment.Id != Id &&
                    segment.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
            if (byOtherSegment is not null)
            {
                problems = problems.Add(new RoadSegmentGeometryTaken(
                    context.Translator.TranslateToOriginalOrTemporaryOrId(byOtherSegment.Id)
                ));
            }
        }

        var checkSegmentBefore = true;
        if (ConvertedFromOutlined && !context.BeforeView.Segments.ContainsKey(Id))
        {
            checkSegmentBefore = false;
        }

        if (checkSegmentBefore)
        {
            var segmentBefore = context.BeforeView.Segments[Id];

            if (segmentBefore.Start != startNodeId && segmentBefore.Start != EndNodeId &&
                context.AfterView.Nodes.TryGetValue(segmentBefore.Start, out var beforeStartNode))
                problems = problems.AddRange(
                    beforeStartNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

            if (segmentBefore.End != startNodeId && segmentBefore.End != endNodeId &&
                context.AfterView.Nodes.TryGetValue(segmentBefore.End, out var beforeEndNode))
                problems = problems.AddRange(
                    beforeEndNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
        }

        if (!context.AfterView.View.Nodes.TryGetValue(startNodeId, out var startNode))
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(startNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line is not null && line.StartPoint != null && !line.StartPoint.IsReasonablyEqualTo(startNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!context.AfterView.View.Nodes.TryGetValue(endNodeId, out var endNode))
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(endNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line is not null && line.EndPoint != null && !line.EndPoint.IsReasonablyEqualTo(endNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentEndPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (line is not null)
        {
            var intersectingSegments = context.AfterView.View.CreateScopedView(line.EnvelopeInternal)
                .FindIntersectingRoadSegments(this, startNodeId, endNodeId);
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

            problems += line.GetProblemsForRoadSegmentLanes(Lanes, context.Tolerances);
            problems += line.GetProblemsForRoadSegmentWidths(Widths, context.Tolerances);
            problems += line.GetProblemsForRoadSegmentSurfaces(Surfaces, context.Tolerances);
        }

        return VerifyAfterResult.WithAcceptedChanges(problems, warnings => BuildAcceptedChanges(warnings, context));
    }

    private IEnumerable<Messages.AcceptedChange> BuildAcceptedChanges(CommandHandling.Actions.ChangeRoadNetwork.ValueObjects.Problem[] warnings, AfterVerificationContext context)
    {
        var afterSegment = context.AfterView.Segments[Id];

        var maintenanceAuthorityId = afterSegment.AttributeHash.OrganizationId;
        var maintainer = context.Organizations.FindAsync(maintenanceAuthorityId, CancellationToken.None).GetAwaiter().GetResult();

        var laneIdentifiers = new Queue<AttributeId>(context.AfterView.View.SegmentReusableLaneAttributeIdentifiers[Id]);
        var widthIdentifiers = new Queue<AttributeId>(context.AfterView.View.SegmentReusableWidthAttributeIdentifiers[Id]);
        var surfaceIdentifiers = new Queue<AttributeId>(context.AfterView.View.SegmentReusableSurfaceAttributeIdentifiers[Id]);

        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentModified = new RoadSegmentModified
            {
                Id = afterSegment.Id,
                OriginalId = OriginalId,
                Version = afterSegment.Version,
                StartNodeId = afterSegment.Start,
                EndNodeId = afterSegment.End,
                Geometry = GeometryTranslator.Translate(afterSegment.Geometry),
                GeometryVersion = afterSegment.GeometryVersion,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = maintenanceAuthorityId,
                    Name = OrganizationName.FromValueWithFallback(maintainer?.Translation.Name)
                },
                GeometryDrawMethod = afterSegment.AttributeHash.GeometryDrawMethod,
                Morphology = afterSegment.AttributeHash.Morphology,
                Status = afterSegment.AttributeHash.Status,
                Category = _correctedCategory ?? afterSegment.AttributeHash.Category,
                AccessRestriction = afterSegment.AttributeHash.AccessRestriction,
                LeftSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = afterSegment.AttributeHash.LeftStreetNameId
                },
                RightSide = new Messages.RoadSegmentSideAttributes
                {
                    StreetNameId = afterSegment.AttributeHash.RightStreetNameId
                },
                Lanes = afterSegment.Lanes
                    .Select(item => new Messages.RoadSegmentLaneAttributes
                    {
                        AttributeId = laneIdentifiers.Dequeue(),
                        AsOfGeometryVersion = ValueObjects.GeometryVersion.Initial,
                        Count = item.Count,
                        Direction = item.Direction,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Widths = afterSegment.Widths
                    .Select(item => new Messages.RoadSegmentWidthAttributes
                    {
                        AttributeId = widthIdentifiers.Dequeue(),
                        AsOfGeometryVersion = ValueObjects.GeometryVersion.Initial,
                        Width = item.Width,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                Surfaces = afterSegment.Surfaces
                    .Select(item => new Messages.RoadSegmentSurfaceAttributes
                    {
                        AttributeId = surfaceIdentifiers.Dequeue(),
                        AsOfGeometryVersion = ValueObjects.GeometryVersion.Initial,
                        Type = item.Type,
                        FromPosition = item.From,
                        ToPosition = item.To
                    })
                    .ToArray(),
                ConvertedFromOutlined = ConvertedFromOutlined
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.ModifyRoadSegment = new Messages.ModifyRoadSegment
        {
            Id = Id,
            OriginalId = OriginalId,
            StartNodeId = TemporaryStartNodeId ?? StartNodeId,
            EndNodeId = TemporaryEndNodeId ?? EndNodeId,
            Geometry = Geometry is not null ? GeometryTranslator.Translate(Geometry) : null,
            MaintenanceAuthority = MaintenanceAuthorityId,
            GeometryDrawMethod = GeometryDrawMethod,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            AccessRestriction = AccessRestriction,
            LeftSideStreetNameId = LeftSideStreetNameId,
            RightSideStreetNameId = RightSideStreetNameId,
            Lanes = Lanes?
                .Select(item => new RequestedRoadSegmentLaneAttribute
                {
                    AttributeId = item.TemporaryId,
                    Count = item.Count,
                    Direction = item.Direction,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Widths = Widths?
                .Select(item => new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = item.TemporaryId,
                    Width = item.Width,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Surfaces = Surfaces?
                .Select(item => new RequestedRoadSegmentSurfaceAttribute
                {
                    AttributeId = item.TemporaryId,
                    Type = item.Type,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            ConvertedFromOutlined = ConvertedFromOutlined
        };
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
