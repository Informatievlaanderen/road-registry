namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using NetTopologySuite.Geometries;

public class ModifyRoadSegment : IRequestedChange, IHaveHash
{
    public const string EventName = "ModifyRoadSegment";

    public ModifyRoadSegment(
        RoadSegmentId id,
        RoadSegmentId? originalId,
        RoadSegmentVersion version,
        RoadNodeId startNodeId,
        RoadNodeId? temporaryStartNodeId,
        RoadNodeId endNodeId,
        RoadNodeId? temporaryEndNodeId,
        MultiLineString geometry,
        GeometryVersion geometryVersion,
        OrganizationId maintenanceAuthorityId,
        OrganizationName? maintenanceAuthorityName,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        RoadSegmentCategory category,
        bool categoryModified,
        RoadSegmentAccessRestriction accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId,
        IReadOnlyList<RoadSegmentLaneAttribute> lanes,
        IReadOnlyList<RoadSegmentWidthAttribute> widths,
        IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces,
        bool convertedFromOutlined)
    {
        Id = id;
        OriginalId = originalId;
        Version = version;
        StartNodeId = startNodeId;
        TemporaryStartNodeId = temporaryStartNodeId;
        EndNodeId = endNodeId;
        TemporaryEndNodeId = temporaryEndNodeId;
        Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        GeometryVersion = geometryVersion;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        MaintenanceAuthorityName = maintenanceAuthorityName;
        GeometryDrawMethod = geometryDrawMethod ?? throw new ArgumentNullException(nameof(geometryDrawMethod));
        Morphology = morphology ?? throw new ArgumentNullException(nameof(morphology));
        Status = status ?? throw new ArgumentNullException(nameof(status));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        CategoryModified = categoryModified;
        AccessRestriction = accessRestriction ?? throw new ArgumentNullException(nameof(accessRestriction));
        LeftSideStreetNameId = leftSideStreetNameId;
        RightSideStreetNameId = rightSideStreetNameId;
        Lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
        Widths = widths ?? throw new ArgumentNullException(nameof(widths));
        Surfaces = surfaces ?? throw new ArgumentNullException(nameof(surfaces));
        ConvertedFromOutlined = convertedFromOutlined;
    }

    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public bool CategoryModified { get; }
    public RoadNodeId EndNodeId { get; }
    public MultiLineString Geometry { get; }
    public GeometryVersion GeometryVersion { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentId? OriginalId { get; }
    public RoadSegmentVersion Version { get; }
    public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
    public StreetNameLocalId? LeftSideStreetNameId { get; }
    public OrganizationId MaintenanceAuthorityId { get; }
    public OrganizationName? MaintenanceAuthorityName { get; }
    public RoadSegmentMorphology Morphology { get; }
    public StreetNameLocalId? RightSideStreetNameId { get; }
    public RoadNodeId StartNodeId { get; }
    public RoadSegmentStatus Status { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public RoadNodeId? TemporaryEndNodeId { get; }
    public RoadNodeId? TemporaryStartNodeId { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }
    public bool ConvertedFromOutlined { get; }
    private RoadSegmentCategory? _correctedCategory;

    public void TranslateTo(Messages.AcceptedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RoadSegmentModified = new RoadSegmentModified
        {
            Id = Id,
            OriginalId = OriginalId,
            Version = Version,
            StartNodeId = StartNodeId,
            EndNodeId = EndNodeId,
            Geometry = GeometryTranslator.Translate(Geometry),
            GeometryVersion = GeometryVersion,
            MaintenanceAuthority = new MaintenanceAuthority
            {
                Code = MaintenanceAuthorityId,
                Name = OrganizationName.FromValueWithFallback(MaintenanceAuthorityName)
            },
            GeometryDrawMethod = GeometryDrawMethod,
            Morphology = Morphology,
            Status = Status,
            Category = _correctedCategory ?? Category,
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
                    AsOfGeometryVersion = GeometryVersion.Initial,
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
                    AsOfGeometryVersion = GeometryVersion.Initial,
                    Width = item.Width,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Surfaces = Surfaces
                .Select(item => new Messages.RoadSegmentSurfaceAttributes
                {
                    AttributeId = item.Id,
                    AsOfGeometryVersion = GeometryVersion.Initial,
                    Type = item.Type,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            ConvertedFromOutlined = ConvertedFromOutlined
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.ModifyRoadSegment = new Messages.ModifyRoadSegment
        {
            Id = Id,
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
                .ToArray(),
            ConvertedFromOutlined = ConvertedFromOutlined
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
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

        var checkSegmentBefore = true;
        if (ConvertedFromOutlined && !context.BeforeView.Segments.ContainsKey(Id))
        {
            checkSegmentBefore = false;
        }

        if (checkSegmentBefore)
        {
            var segmentBefore = context.BeforeView.Segments[Id];

            if (segmentBefore.Start != StartNodeId && segmentBefore.Start != EndNodeId &&
                context.AfterView.Nodes.TryGetValue(segmentBefore.Start, out var beforeStartNode))
                problems = problems.AddRange(
                    beforeStartNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

            if (segmentBefore.End != StartNodeId && segmentBefore.End != EndNodeId &&
                context.AfterView.Nodes.TryGetValue(segmentBefore.End, out var beforeEndNode))
                problems = problems.AddRange(
                    beforeEndNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
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
        return problems;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;
        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);

        if (!context.BeforeView.Segments.TryGetValue(Id, out var currentSegment) && !ConvertedFromOutlined)
        {
            problems = problems.Add(new RoadSegmentNotFound(originalIdOrId));
        }

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

        if (currentSegment is not null && !CategoryModified && RoadSegmentCategory.IsUpgraded(currentSegment.AttributeHash.Category))
        {
            _correctedCategory = currentSegment.AttributeHash.Category;
            problems += new RoadSegmentCategoryNotChangedBecauseAlreadyIsNewerVersion(originalIdOrId);
        }

        return problems;
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
