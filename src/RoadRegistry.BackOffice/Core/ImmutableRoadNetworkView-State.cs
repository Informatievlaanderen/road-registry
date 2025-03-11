namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Messages;

public partial class ImmutableRoadNetworkView
{
    public IRoadNetworkView RestoreFromEvent(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        return RestoreFromEvents([@event]);
    }

    public IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var result = this;
        var eventIndex = 0;

        foreach (var @event in events)
        {
            try
            {
                switch (@event)
                {
                    case ImportedRoadNode importedRoadNode:
                        result = result.Given(importedRoadNode);
                        break;
                    case ImportedRoadSegment importedRoadSegment:
                        result = result.Given(importedRoadSegment);
                        break;
                    case ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                        result = result.Given(importedGradeSeparatedJunction);
                        break;
                    case RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                        result = result.Given(roadNetworkChangesAccepted);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed trying to process the message with index {eventIndex} of type '{@event.GetType().Name}': {ex.Message}", ex);
            }

            eventIndex++;
        }

        return result;
    }

    public IRoadNetworkView RestoreFromSnapshot(RoadNetworkSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new ImmutableRoadNetworkView(
            snapshot.Nodes.ToImmutableDictionary(node => new RoadNodeId(node.Id),
                node =>
                {
                    var roadNode = new RoadNode(new RoadNodeId(node.Id), new RoadNodeVersion(node.Version), RoadNodeType.Parse(node.Type),
                        GeometryTranslator.Translate(node.Geometry));

                    return node.Segments.Aggregate(roadNode, (current, segment) => current.ConnectWith(new RoadSegmentId(segment)));
                }),
            snapshot.Segments.ToImmutableDictionary(segment => new RoadSegmentId(segment.Id),
                segment =>
                {
                    var roadSegment = new RoadSegment(
                        new RoadSegmentId(segment.Id),
                        new RoadSegmentVersion(segment.Version),
                        GeometryTranslator.Translate(segment.Geometry),
                        new GeometryVersion(segment.GeometryVersion),
                        new RoadNodeId(segment.StartNodeId),
                        new RoadNodeId(segment.EndNodeId),
                        new AttributeHash(
                            RoadSegmentAccessRestriction.Parse(segment.AttributeHash.AccessRestriction),
                            RoadSegmentCategory.Parse(segment.AttributeHash.Category),
                            RoadSegmentMorphology.Parse(segment.AttributeHash.Morphology),
                            RoadSegmentStatus.Parse(segment.AttributeHash.Status),
                            StreetNameLocalId.FromValue(segment.AttributeHash.LeftSideStreetNameId),
                            StreetNameLocalId.FromValue(segment.AttributeHash.RightSideStreetNameId),
                            new OrganizationId(segment.AttributeHash.OrganizationId),
                            RoadSegmentGeometryDrawMethod.Parse(segment.AttributeHash.GeometryDrawMethod)
                        ),
                        segment.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                            new RoadSegmentPosition(lane.FromPosition),
                            new RoadSegmentPosition(lane.ToPosition),
                            new RoadSegmentLaneCount(lane.Count),
                            RoadSegmentLaneDirection.ByIdentifier[lane.Direction],
                            new GeometryVersion(lane.AsOfGeometryVersion))).ToArray(),
                        segment.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                            new RoadSegmentPosition(surface.FromPosition),
                            new RoadSegmentPosition(surface.ToPosition),
                            RoadSegmentSurfaceType.ByIdentifier[surface.Type],
                            new GeometryVersion(surface.AsOfGeometryVersion))).ToArray(),
                        segment.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                            new RoadSegmentPosition(width.FromPosition),
                            new RoadSegmentPosition(width.ToPosition),
                            new RoadSegmentWidth(width.Width),
                            new GeometryVersion(width.AsOfGeometryVersion))).ToArray(),
                        segment.LastEventHash
                    );
                    roadSegment = segment.EuropeanRoadAttributes.Aggregate(roadSegment, (current, attribute) =>
                        current.PartOfEuropeanRoad(new RoadSegmentEuropeanRoadAttribute(
                            new AttributeId(attribute.AttributeId),
                            EuropeanRoadNumber.Parse(attribute.Number)
                        )));
                    roadSegment = segment.NationalRoadAttributes.Aggregate(roadSegment, (current, attribute) =>
                        current.PartOfNationalRoad(new RoadSegmentNationalRoadAttribute(
                            new AttributeId(attribute.AttributeId),
                            NationalRoadNumber.Parse(attribute.Number)
                        )));
                    roadSegment = segment.NumberedRoadAttributes.Aggregate(roadSegment, (current, attribute) =>
                        current.PartOfNumberedRoad(new RoadSegmentNumberedRoadAttribute(
                            new AttributeId(attribute.AttributeId),
                            RoadSegmentNumberedRoadDirection.Parse(attribute.Direction),
                            NumberedRoadNumber.Parse(attribute.Number),
                            new RoadSegmentNumberedRoadOrdinal(attribute.Ordinal)
                        )));
                    return roadSegment;
                }),
            snapshot.GradeSeparatedJunctions.ToImmutableDictionary(gradeSeparatedJunction => new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                gradeSeparatedJunction => new GradeSeparatedJunction(
                    new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                    GradeSeparatedJunctionType.Parse(gradeSeparatedJunction.Type),
                    new RoadSegmentId(gradeSeparatedJunction.UpperSegmentId),
                    new RoadSegmentId(gradeSeparatedJunction.LowerSegmentId))),
            snapshot.SegmentReusableLaneAttributeIdentifiers.ToImmutableDictionary(
                segment => new RoadSegmentId(segment.SegmentId),
                segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                    .Select(identifier => new AttributeId(identifier)).ToArray()),
            snapshot.SegmentReusableWidthAttributeIdentifiers.ToImmutableDictionary(
                segment => new RoadSegmentId(segment.SegmentId),
                segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                    .Select(identifier => new AttributeId(identifier)).ToArray()),
            snapshot.SegmentReusableSurfaceAttributeIdentifiers.ToImmutableDictionary(
                segment => new RoadSegmentId(segment.SegmentId),
                segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                    .Select(identifier => new AttributeId(identifier)).ToArray())
        );
    }

    private ImmutableRoadNetworkView Given(ImportedRoadNode @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var id = new RoadNodeId(@event.Id);
        var version = new RoadNodeVersion(@event.Version);
        var type = RoadNodeType.Parse(@event.Type);
        var node = new RoadNode(id, version, type, GeometryTranslator.Translate(@event.Geometry));
        return new ImmutableRoadNetworkView(
            _nodes.Add(id, node),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(ImportedRoadSegment @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var id = new RoadSegmentId(@event.Id);
        var version = new RoadSegmentVersion(@event.Version);
        var start = new RoadNodeId(@event.StartNodeId);
        var end = new RoadNodeId(@event.EndNodeId);
        var geometryVersion = new GeometryVersion(@event.GeometryVersion);

        var attributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            StreetNameLocalId.FromValue(@event.LeftSide.StreetNameId),
            StreetNameLocalId.FromValue(@event.RightSide.StreetNameId),
            new OrganizationId(@event.MaintenanceAuthority.Code),
            RoadSegmentGeometryDrawMethod.Parse(@event.GeometryDrawMethod)
        );

        var segment = new RoadSegment(
            id,
            version,
            GeometryTranslator.Translate(@event.Geometry),
            geometryVersion,
            start,
            end,
            attributeHash,
            @event.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                new RoadSegmentPosition(lane.FromPosition),
                new RoadSegmentPosition(lane.ToPosition),
                new RoadSegmentLaneCount(lane.Count),
                RoadSegmentLaneDirection.Parse(lane.Direction),
                new GeometryVersion(lane.AsOfGeometryVersion))).ToArray(),
            @event.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                new RoadSegmentPosition(surface.FromPosition),
                new RoadSegmentPosition(surface.ToPosition),
                RoadSegmentSurfaceType.Parse(surface.Type),
                new GeometryVersion(surface.AsOfGeometryVersion))).ToArray(),
            @event.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                new RoadSegmentPosition(width.FromPosition),
                new RoadSegmentPosition(width.ToPosition),
                new RoadSegmentWidth(width.Width),
                new GeometryVersion(width.AsOfGeometryVersion))).ToArray(),
            @event.GetHash());
        segment = @event.PartOfEuropeanRoads.Aggregate(segment, (current, europeanRoad) =>
            current.PartOfEuropeanRoad(new RoadSegmentEuropeanRoadAttribute(
                new AttributeId(europeanRoad.AttributeId),
                EuropeanRoadNumber.Parse(europeanRoad.Number)
            )));
        segment = @event.PartOfNationalRoads.Aggregate(segment, (current, nationalRoad) =>
            current.PartOfNationalRoad(new RoadSegmentNationalRoadAttribute(
                new AttributeId(nationalRoad.AttributeId),
                NationalRoadNumber.Parse(nationalRoad.Number)
            )));
        segment = @event.PartOfNumberedRoads.Aggregate(segment, (current, numberedRoad) =>
            current.PartOfNumberedRoad(new RoadSegmentNumberedRoadAttribute(
                new AttributeId(numberedRoad.AttributeId),
                RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction),
                NumberedRoadNumber.Parse(numberedRoad.Number),
                new RoadSegmentNumberedRoadOrdinal(numberedRoad.Ordinal)
            )));

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id)),
            _segments.Add(id, segment),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
            SegmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
        );
    }

    private ImmutableRoadNetworkView Given(ImportedGradeSeparatedJunction @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            ),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadNetworkChangesAccepted @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var result = new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
        foreach (var change in @event.Changes.Flatten())
            switch (change)
            {
                case RoadNodeAdded roadNodeAdded:
                    result = result.Given(roadNodeAdded);
                    break;
                case RoadNodeModified roadNodeModified:
                    result = result.Given(roadNodeModified);
                    break;
                case RoadNodeRemoved roadNodeRemoved:
                    result = result.Given(roadNodeRemoved);
                    break;
                case RoadSegmentAdded roadSegmentAdded:
                    result = result.Given(roadSegmentAdded);
                    break;
                case RoadSegmentModified roadSegmentModified:
                    result = result.Given(roadSegmentModified);
                    break;
                case RoadSegmentAttributesModified roadSegmentAttributesModified:
                    result = result.Given(roadSegmentAttributesModified);
                    break;
                case RoadSegmentGeometryModified roadSegmentGeometryModified:
                    result = result.Given(roadSegmentGeometryModified);
                    break;
                case RoadSegmentRemoved roadSegmentRemoved:
                    result = result.Given(roadSegmentRemoved);
                    break;
                case RoadSegmentsRemoved roadSegmentRemoved:
                    result = result.Given(roadSegmentRemoved);
                    break;
                case OutlinedRoadSegmentRemoved roadSegmentOutlinedRemoved:
                    result = result.Given(roadSegmentOutlinedRemoved);
                    break;
                case RoadSegmentAddedToEuropeanRoad roadSegmentAddedToEuropeanRoad:
                    result = result.Given(roadSegmentAddedToEuropeanRoad);
                    break;
                case RoadSegmentRemovedFromEuropeanRoad roadSegmentRemovedFromEuropeanRoad:
                    result = result.Given(roadSegmentRemovedFromEuropeanRoad);
                    break;
                case RoadSegmentAddedToNationalRoad roadSegmentAddedToNationalRoad:
                    result = result.Given(roadSegmentAddedToNationalRoad);
                    break;
                case RoadSegmentRemovedFromNationalRoad roadSegmentRemovedFromNationalRoad:
                    result = result.Given(roadSegmentRemovedFromNationalRoad);
                    break;
                case RoadSegmentAddedToNumberedRoad roadSegmentAddedToNumberedRoad:
                    result = result.Given(roadSegmentAddedToNumberedRoad);
                    break;
                case RoadSegmentRemovedFromNumberedRoad roadSegmentRemovedFromNumberedRoad:
                    result = result.Given(roadSegmentRemovedFromNumberedRoad);
                    break;
                case GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
                    result = result.Given(gradeSeparatedJunctionAdded);
                    break;
                case GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
                    result = result.Given(gradeSeparatedJunctionModified);
                    break;
                case GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved:
                    result = result.Given(gradeSeparatedJunctionRemoved);
                    break;
            }

        return result;
    }

    private ImmutableRoadNetworkView Given(RoadNodeAdded @event)
    {
        var id = new RoadNodeId(@event.Id);
        var version = new RoadNodeVersion(@event.Version);
        var type = RoadNodeType.Parse(@event.Type);
        var node = new RoadNode(id, version, type, GeometryTranslator.Translate(@event.Geometry));
        return new ImmutableRoadNetworkView(
            _nodes.Add(id, node),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadNodeModified @event)
    {
        var id = new RoadNodeId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes.TryReplace(id, node => node
                .WithGeometry(GeometryTranslator.Translate(@event.Geometry))
                .WithType(RoadNodeType.Parse(@event.Type))
                .WithVersion(new RoadNodeVersion(@event.Version)) // todo-pr how to calculate roadnodeversion?
            ),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadNodeRemoved @event)
    {
        var id = new RoadNodeId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes.Remove(id),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAdded @event)
    {
        var id = new RoadSegmentId(@event.Id);
        var version = new RoadSegmentVersion(@event.Version);
        var start = new RoadNodeId(@event.StartNodeId);
        var end = new RoadNodeId(@event.EndNodeId);
        var geometryVersion = new GeometryVersion(@event.GeometryVersion);

        var attributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            StreetNameLocalId.FromValue(@event.LeftSide.StreetNameId),
            StreetNameLocalId.FromValue(@event.RightSide.StreetNameId),
            new OrganizationId(@event.MaintenanceAuthority.Code),
            RoadSegmentGeometryDrawMethod.Parse(@event.GeometryDrawMethod)
        );

        var segment = new RoadSegment(
            id,
            version,
            GeometryTranslator.Translate(@event.Geometry),
            geometryVersion,
            start,
            end,
            attributeHash,
            @event.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                new RoadSegmentPosition(lane.FromPosition),
                new RoadSegmentPosition(lane.ToPosition),
                new RoadSegmentLaneCount(lane.Count),
                RoadSegmentLaneDirection.Parse(lane.Direction),
                new GeometryVersion(lane.AsOfGeometryVersion))).ToArray(),
            @event.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                new RoadSegmentPosition(surface.FromPosition),
                new RoadSegmentPosition(surface.ToPosition),
                RoadSegmentSurfaceType.Parse(surface.Type),
                new GeometryVersion(surface.AsOfGeometryVersion))).ToArray(),
            @event.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                new RoadSegmentPosition(width.FromPosition),
                new RoadSegmentPosition(width.ToPosition),
                new RoadSegmentWidth(width.Width),
                new GeometryVersion(width.AsOfGeometryVersion))).ToArray(),
            @event.GetHash());

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id)),
            _segments.Add(id, segment),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
            SegmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
        );
    }

    private ImmutableRoadNetworkView Given(RoadSegmentModified @event)
    {
        var view = this;

        var id = new RoadSegmentId(@event.Id);

        if (@event.ConvertedFromOutlined && !_segments.ContainsKey(id))
        {
            view = Given(new RoadSegmentAdded
            {
                Id = @event.Id,
                AccessRestriction = @event.AccessRestriction,
                Category = @event.Category,
                EndNodeId = @event.EndNodeId,
                Geometry = @event.Geometry,
                GeometryDrawMethod = @event.GeometryDrawMethod,
                GeometryVersion = @event.GeometryVersion,
                Lanes = @event.Lanes,
                LeftSide = @event.LeftSide,
                MaintenanceAuthority = @event.MaintenanceAuthority,
                Morphology = @event.Morphology,
                RightSide = @event.RightSide,
                StartNodeId = @event.StartNodeId,
                Status = @event.Status,
                Surfaces = @event.Surfaces,
                TemporaryId = @event.Id,
                OriginalId = @event.Id,
                Version = @event.Version,
                Widths = @event.Widths
            });
        }

        var version = new RoadSegmentVersion(@event.Version);
        var start = new RoadNodeId(@event.StartNodeId);
        var end = new RoadNodeId(@event.EndNodeId);
        var geometryVersion = new GeometryVersion(@event.GeometryVersion);

        var attributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            StreetNameLocalId.FromValue(@event.LeftSide.StreetNameId),
            StreetNameLocalId.FromValue(@event.RightSide.StreetNameId),
            new OrganizationId(@event.MaintenanceAuthority.Code),
            RoadSegmentGeometryDrawMethod.Parse(@event.GeometryDrawMethod)
        );

        var segmentBefore = view._segments[id];

        return new ImmutableRoadNetworkView(
            view._nodes
                .TryReplaceIf(segmentBefore.Start, node => node.Id != start, node => node.DisconnectFrom(id))
                .TryReplaceIf(segmentBefore.End, node => node.Id != end, node => node.DisconnectFrom(id))
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id)),
            view._segments
                .TryReplace(id, segment => segment
                    .WithVersion(version)
                    .WithGeometry(GeometryTranslator.Translate(@event.Geometry))
                    .WithGeometryVersion(geometryVersion)
                    .WithStartAndEndAndAttributeHash(start, end, attributeHash)
                    .WithLanes(@event.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                        new RoadSegmentPosition(lane.FromPosition),
                        new RoadSegmentPosition(lane.ToPosition),
                        new RoadSegmentLaneCount(lane.Count),
                        RoadSegmentLaneDirection.Parse(lane.Direction),
                        new GeometryVersion(lane.AsOfGeometryVersion))).ToArray())
                    .WithSurfaces(@event.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                        new RoadSegmentPosition(surface.FromPosition),
                        new RoadSegmentPosition(surface.ToPosition),
                        RoadSegmentSurfaceType.Parse(surface.Type),
                        new GeometryVersion(surface.AsOfGeometryVersion))).ToArray())
                    .WithWidths(@event.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                        new RoadSegmentPosition(width.FromPosition),
                        new RoadSegmentPosition(width.ToPosition),
                        new RoadSegmentWidth(width.Width),
                        new GeometryVersion(width.AsOfGeometryVersion))).ToArray())
                    .WithLastEventHash(@event.GetHash())
                ),
            view._gradeSeparatedJunctions,
            view.SegmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
            view.SegmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId))),
            view.SegmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
        );
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAttributesModified @event)
    {
        var id = new RoadSegmentId(@event.Id);
        var version = new RoadSegmentVersion(@event.Version);

        var segmentBefore = _segments[id];

        var attributeHash = new AttributeHash(
            @event.AccessRestriction is not null
                ? RoadSegmentAccessRestriction.Parse(@event.AccessRestriction)
                : segmentBefore.AttributeHash.AccessRestriction,
            @event.Category is not null
                ? RoadSegmentCategory.Parse(@event.Category)
                : segmentBefore.AttributeHash.Category,
            @event.Morphology is not null
                ? RoadSegmentMorphology.Parse(@event.Morphology)
                : segmentBefore.AttributeHash.Morphology,
            @event.Status is not null
                ? RoadSegmentStatus.Parse(@event.Status)
                : segmentBefore.AttributeHash.Status,
            @event.LeftSide is not null
                ? StreetNameLocalId.FromValue(@event.LeftSide.StreetNameId)
                : segmentBefore.AttributeHash.LeftStreetNameId,
            @event.RightSide is not null
                ? StreetNameLocalId.FromValue(@event.RightSide.StreetNameId)
                : segmentBefore.AttributeHash.RightStreetNameId,
            @event.MaintenanceAuthority is not null
                ? new OrganizationId(@event.MaintenanceAuthority.Code)
                : segmentBefore.AttributeHash.OrganizationId,
            segmentBefore.AttributeHash.GeometryDrawMethod
        );

        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(id, segment => segment
                    .WithVersion(version)
                    .WithAttributeHash(attributeHash)
                    .WithLanes(@event.Lanes?
                        .Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                            new RoadSegmentPosition(lane.FromPosition),
                            new RoadSegmentPosition(lane.ToPosition),
                            new RoadSegmentLaneCount(lane.Count),
                            RoadSegmentLaneDirection.Parse(lane.Direction),
                            new GeometryVersion(lane.AsOfGeometryVersion)))
                        .ToArray() ?? segmentBefore.Lanes)
                    .WithSurfaces(@event.Surfaces?
                        .Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                            new RoadSegmentPosition(surface.FromPosition),
                            new RoadSegmentPosition(surface.ToPosition),
                            RoadSegmentSurfaceType.Parse(surface.Type),
                            new GeometryVersion(surface.AsOfGeometryVersion)))
                        .ToArray() ?? segmentBefore.Surfaces)
                    .WithWidths(@event.Widths?
                        .Select(width => new BackOffice.RoadSegmentWidthAttribute(
                            new RoadSegmentPosition(width.FromPosition),
                            new RoadSegmentPosition(width.ToPosition),
                            new RoadSegmentWidth(width.Width),
                            new GeometryVersion(width.AsOfGeometryVersion)))
                        .ToArray() ?? segmentBefore.Widths)
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView Given(RoadSegmentGeometryModified @event)
    {
        var id = new RoadSegmentId(@event.Id);
        var version = new RoadSegmentVersion(@event.Version);
        var geometryVersion = new GeometryVersion(@event.GeometryVersion);

        var segmentBefore = _segments[id];

        var attributeHash = new AttributeHash(
            segmentBefore.AttributeHash.AccessRestriction,
            segmentBefore.AttributeHash.Category,
            segmentBefore.AttributeHash.Morphology,
            segmentBefore.AttributeHash.Status,
            segmentBefore.AttributeHash.LeftStreetNameId,
            segmentBefore.AttributeHash.RightStreetNameId,
            segmentBefore.AttributeHash.OrganizationId,
            segmentBefore.AttributeHash.GeometryDrawMethod
        );

        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(id, segment => segment
                    .WithVersion(version)
                    .WithGeometryVersion(geometryVersion)
                    .WithGeometry(GeometryTranslator.Translate(@event.Geometry))
                    .WithAttributeHash(attributeHash)
                    .WithLanes(@event.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                        new RoadSegmentPosition(lane.FromPosition),
                        new RoadSegmentPosition(lane.ToPosition),
                        new RoadSegmentLaneCount(lane.Count),
                        RoadSegmentLaneDirection.Parse(lane.Direction),
                        new GeometryVersion(lane.AsOfGeometryVersion))).ToArray())
                    .WithSurfaces(@event.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                        new RoadSegmentPosition(surface.FromPosition),
                        new RoadSegmentPosition(surface.ToPosition),
                        RoadSegmentSurfaceType.Parse(surface.Type),
                        new GeometryVersion(surface.AsOfGeometryVersion))).ToArray())
                    .WithWidths(@event.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                        new RoadSegmentPosition(width.FromPosition),
                        new RoadSegmentPosition(width.ToPosition),
                        new RoadSegmentWidth(width.Width),
                        new GeometryVersion(width.AsOfGeometryVersion))).ToArray())
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
            SegmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
        );
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemoved @event)
    {
        var id = new RoadSegmentId(@event.Id);
        return new ImmutableRoadNetworkView(
            _segments.TryGetValue(id, out var segment)
                ? _nodes
                    .TryReplace(segment.Start, node => node.DisconnectFrom(id))
                    .TryReplace(segment.End, node => node.DisconnectFrom(id))
                : _nodes,
            _segments.Remove(id),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView Given(RoadSegmentsRemoved @event)
    {
        var segments = _segments;
        var nodes = _nodes;
        foreach (var roadSegmentId in @event.RemovedRoadSegmentIds.Select(x => new RoadSegmentId(x)))
        {
            _segments.TryGetValue(roadSegmentId, out var segment);

            segments = _segments.Remove(roadSegmentId);
            nodes = nodes.TryReplace(segment!.Start, node => node.DisconnectFrom(roadSegmentId));
            nodes = nodes.TryReplace(segment.End, node => node.DisconnectFrom(roadSegmentId));
        }

        foreach (var roadNodeId in @event.RemovedRoadNodeIds.Select(x => new RoadNodeId(x)))
        {
            nodes = nodes.Remove(roadNodeId);
        }

        foreach (var changedRoadNode in @event.ChangedRoadNodes)
        {
           nodes = nodes.TryReplace(new RoadNodeId(changedRoadNode.Id), node => node
                .WithType(RoadNodeType.Parse(changedRoadNode.Type))
                .WithVersion(new RoadNodeVersion(changedRoadNode.Version))
            );
        }

        var junctions = _gradeSeparatedJunctions;
        foreach (var gradeSeparatedJunctionId in @event.RemovedGradeSeparatedJunctionIds.Select(x => new GradeSeparatedJunctionId(x)))
        {
            junctions = junctions.Remove(gradeSeparatedJunctionId);
        }

        return new ImmutableRoadNetworkView(
            nodes,
            segments,
            junctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView Given(OutlinedRoadSegmentRemoved @event)
    {
        var id = new RoadSegmentId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryGetValue(id, out var segment) && segment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined
                ? _segments.Remove(id)
                : _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView Given(GradeSeparatedJunctionAdded @event)
    {
        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            ),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(GradeSeparatedJunctionModified @event)
    {
        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.TryReplace(
                id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(GradeSeparatedJunctionType.Parse(@event.Type))
                        .WithUpperSegment(new RoadSegmentId(@event.UpperRoadSegmentId))
                        .WithLowerSegment(new RoadSegmentId(@event.LowerRoadSegmentId))
            ),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(GradeSeparatedJunctionRemoved @event)
    {
        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Remove(id),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAddedToEuropeanRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment
                    .PartOfEuropeanRoad(new RoadSegmentEuropeanRoadAttribute(
                        new AttributeId(@event.AttributeId),
                        EuropeanRoadNumber.Parse(@event.Number)
                    ))
                    .WithVersion(new RoadSegmentVersion(@event.SegmentVersion ?? segment.Version))
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemovedFromEuropeanRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment
                    .NotPartOfEuropeanRoad(new AttributeId(@event.AttributeId))
                    .WithVersion(new RoadSegmentVersion(@event.SegmentVersion ?? segment.Version))
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAddedToNationalRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment
                    .PartOfNationalRoad(new RoadSegmentNationalRoadAttribute(
                        new AttributeId(@event.AttributeId),
                        NationalRoadNumber.Parse(@event.Number)
                    ))
                    .WithVersion(new RoadSegmentVersion(@event.SegmentVersion ?? segment.Version))
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemovedFromNationalRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment
                    .NotPartOfNationalRoad(new AttributeId(@event.AttributeId))
                    .WithVersion(new RoadSegmentVersion(@event.SegmentVersion ?? segment.Version))
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAddedToNumberedRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment
                    .PartOfNumberedRoad(new RoadSegmentNumberedRoadAttribute(
                        new AttributeId(@event.AttributeId),
                        RoadSegmentNumberedRoadDirection.Parse(@event.Direction),
                        NumberedRoadNumber.Parse(@event.Number),
                        new RoadSegmentNumberedRoadOrdinal(@event.Ordinal)
                    ))
                    .WithVersion(new RoadSegmentVersion(@event.SegmentVersion ?? segment.Version))
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemovedFromNumberedRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment
                    .NotPartOfNumberedRoad(new AttributeId(@event.AttributeId))
                    .WithVersion(new RoadSegmentVersion(@event.SegmentVersion ?? segment.Version))
                    .WithLastEventHash(@event.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }
}
