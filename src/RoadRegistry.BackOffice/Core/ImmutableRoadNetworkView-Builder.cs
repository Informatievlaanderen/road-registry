namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Messages;
using NetTopologySuite.Geometries;

public partial class ImmutableRoadNetworkView
{
    private sealed class Builder : IRoadNetworkView
    {
        private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction>.Builder _gradeSeparatedJunctions;
        private readonly ImmutableDictionary<RoadNodeId, RoadNode>.Builder _nodes;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
            _segmentReusableLaneAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
            _segmentReusableSurfaceAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
            _segmentReusableWidthAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, RoadSegment>.Builder _segments;

        public Builder(
            ImmutableDictionary<RoadNodeId, RoadNode>.Builder nodes,
            ImmutableDictionary<RoadSegmentId, RoadSegment>.Builder segments,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction>.Builder gradeSeparatedJunctions,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                segmentReusableLaneAttributeIdentifiers,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                segmentReusableWidthAttributeIdentifiers,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                segmentReusableSurfaceAttributeIdentifiers)
        {
            _nodes = nodes;
            _segments = segments;
            _gradeSeparatedJunctions = gradeSeparatedJunctions;
            _segmentReusableLaneAttributeIdentifiers = segmentReusableLaneAttributeIdentifiers;
            _segmentReusableWidthAttributeIdentifiers = segmentReusableWidthAttributeIdentifiers;
            _segmentReusableSurfaceAttributeIdentifiers = segmentReusableSurfaceAttributeIdentifiers;
        }

        public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions => _gradeSeparatedJunctions.ToImmutable();
        public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes.ToImmutable();

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            SegmentReusableLaneAttributeIdentifiers =>
            _segmentReusableLaneAttributeIdentifiers.ToImmutable();

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            SegmentReusableSurfaceAttributeIdentifiers =>
            _segmentReusableSurfaceAttributeIdentifiers.ToImmutable();

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            SegmentReusableWidthAttributeIdentifiers =>
            _segmentReusableWidthAttributeIdentifiers.ToImmutable();

        public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments.ToImmutable();

        public IScopedRoadNetworkView CreateScopedView(Envelope envelope)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            // Any nodes that the envelope contains
            var nodes = Nodes
                .Where(pair => envelope.Contains(pair.Value.Geometry.Coordinate))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            // Any segments that intersect the envelope
            var segments = Segments
                .Where(pair => envelope.Intersects(pair.Value.Geometry.EnvelopeInternal))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            // Any junctions for which either the lower or the upper segment intersects the envelope
            var junctions = GradeSeparatedJunctions
                .Where(pair => envelope.Intersects(Segments[pair.Value.LowerSegment].Geometry.EnvelopeInternal) || envelope.Intersects(Segments[pair.Value.UpperSegment].Geometry.EnvelopeInternal))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return new ImmutableScopedRoadNetworkView(
                envelope,
                nodes,
                segments,
                junctions,
                this);
        }

        public IRoadNetworkView RestoreFromEvent(object @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return RestoreFromEvents(new[] { @event });
        }

        public IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events)
        {
            ArgumentNullException.ThrowIfNull(events);

            var eventIndex = 0;

            foreach (var @event in events)
            {
                try
                {
                    switch (@event)
                    {
                        case ImportedRoadNode importedRoadNode:
                            Given(importedRoadNode);
                            break;
                        case ImportedRoadSegment importedRoadSegment:
                            Given(importedRoadSegment);
                            break;
                        case ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                            Given(importedGradeSeparatedJunction);
                            break;
                        case RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                            Given(roadNetworkChangesAccepted);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed trying to process the message with index {eventIndex} of type '{@event.GetType().Name}': {ex.Message}", ex);
                }

                eventIndex++;
            }

            return this;
        }

        public IRoadNetworkView RestoreFromSnapshot(RoadNetworkSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            return new Builder(
                snapshot.Nodes.ToImmutableDictionary(
                    node => new RoadNodeId(node.Id),
                    node =>
                    {
                        var roadNode = new RoadNode(new RoadNodeId(node.Id), new RoadNodeVersion(node.Version), RoadNodeType.Parse(node.Type),
                            GeometryTranslator.Translate(node.Geometry));

                        return node.Segments.Aggregate(roadNode, (current, segment) => current.ConnectWith(new RoadSegmentId(segment)));
                    }).ToBuilder(),
                snapshot.Segments.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.Id),
                    segment =>
                    {
                        var roadSegment = new RoadSegment(new RoadSegmentId(segment.Id),
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
                            segment.LastEventHash);
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
                    }).ToBuilder(),
                snapshot.GradeSeparatedJunctions.ToImmutableDictionary(gradeSeparatedJunction => new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                    gradeSeparatedJunction => new GradeSeparatedJunction(
                        new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                        GradeSeparatedJunctionType.Parse(gradeSeparatedJunction.Type),
                        new RoadSegmentId(gradeSeparatedJunction.UpperSegmentId),
                        new RoadSegmentId(gradeSeparatedJunction.LowerSegmentId))).ToBuilder(),
                snapshot.SegmentReusableLaneAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder(),
                snapshot.SegmentReusableWidthAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder(),
                snapshot.SegmentReusableSurfaceAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder()
            );
        }

        public RoadNetworkSnapshot TakeSnapshot()
        {
            return new RoadNetworkSnapshot
            {
                Nodes = _nodes.Select(node => new RoadNetworkSnapshotNode
                {
                    Id = node.Value.Id.ToInt32(),
                    Segments = node.Value.Segments.Select(segment => segment.ToInt32()).ToArray(),
                    Type = node.Value.Type,
                    Geometry = GeometryTranslator.Translate(node.Value.Geometry),
                    Version = node.Value.Version
                }).ToArray(),
                Segments = _segments.Select(segment => new RoadNetworkSnapshotSegment
                {
                    Id = segment.Value.Id.ToInt32(),
                    Version = segment.Value.Version.ToInt32(),
                    StartNodeId = segment.Value.Start.ToInt32(),
                    EndNodeId = segment.Value.End.ToInt32(),
                    Geometry = GeometryTranslator.Translate(segment.Value.Geometry),
                    GeometryVersion = segment.Value.GeometryVersion.ToInt32(),
                    AttributeHash = new RoadNetworkSnapshotSegmentAttributeHash
                    {
                        AccessRestriction = segment.Value.AttributeHash.AccessRestriction,
                        Category = segment.Value.AttributeHash.Category,
                        Morphology = segment.Value.AttributeHash.Morphology,
                        Status = segment.Value.AttributeHash.Status,
                        LeftSideStreetNameId = segment.Value.AttributeHash.LeftStreetNameId?.ToInt32(),
                        RightSideStreetNameId = segment.Value.AttributeHash.RightStreetNameId?.ToInt32(),
                        OrganizationId = segment.Value.AttributeHash.OrganizationId,
                        GeometryDrawMethod = segment.Value.AttributeHash.GeometryDrawMethod.ToString()
                    },
                    EuropeanRoadAttributes = segment.Value.EuropeanRoadAttributes.Select(europeanRoad => new RoadNetworkSnapshotSegmentEuropeanRoadAttribute
                    {
                        AttributeId = europeanRoad.Value.AttributeId,
                        Number = europeanRoad.Value.Number
                    }).ToArray(),
                    NationalRoadAttributes = segment.Value.NationalRoadAttributes.Select(nationalRoad => new RoadNetworkSnapshotSegmentNationalRoadAttribute
                    {
                        AttributeId = nationalRoad.Value.AttributeId,
                        Number = nationalRoad.Value.Number
                    }).ToArray(),
                    NumberedRoadAttributes = segment.Value.NumberedRoadAttributes.Select(numberedRoad => new RoadNetworkSnapshotSegmentNumberedRoadAttribute
                    {
                        AttributeId = numberedRoad.Value.AttributeId,
                        Direction = numberedRoad.Value.Direction,
                        Number = numberedRoad.Value.Number,
                        Ordinal = numberedRoad.Value.Ordinal
                    }).ToArray(),
                    Lanes = segment.Value.Lanes.Select(lane => new RoadNetworkSnapshotSegmentLaneAttribute
                    {
                        FromPosition = lane.From.ToDecimal(),
                        ToPosition = lane.To.ToDecimal(),
                        Count = lane.Count.ToInt32(),
                        Direction = lane.Direction.Translation.Identifier,
                        AsOfGeometryVersion = lane.AsOfGeometryVersion
                    }).ToArray(),
                    Surfaces = segment.Value.Surfaces.Select(surface => new RoadNetworkSnapshotSegmentSurfaceAttribute
                    {
                        FromPosition = surface.From.ToDecimal(),
                        ToPosition = surface.To.ToDecimal(),
                        Type = surface.Type.Translation.Identifier,
                        AsOfGeometryVersion = surface.AsOfGeometryVersion
                    }).ToArray(),
                    Widths = segment.Value.Widths.Select(width => new RoadNetworkSnapshotSegmentWidthAttribute
                    {
                        FromPosition = width.From.ToDecimal(),
                        ToPosition = width.To.ToDecimal(),
                        Width = width.Width.ToInt32(),
                        AsOfGeometryVersion = width.AsOfGeometryVersion
                    }).ToArray(),
                    LastEventHash = segment.Value.LastEventHash
                }).ToArray(),
                GradeSeparatedJunctions = _gradeSeparatedJunctions.Select(gradeSeparatedJunction => new RoadNetworkSnapshotGradeSeparatedJunction
                {
                    Id = gradeSeparatedJunction.Value.Id,
                    Type = gradeSeparatedJunction.Value.Type,
                    UpperSegmentId = gradeSeparatedJunction.Value.UpperSegment,
                    LowerSegmentId = gradeSeparatedJunction.Value.LowerSegment
                }).ToArray(),
                SegmentReusableLaneAttributeIdentifiers = _segmentReusableLaneAttributeIdentifiers.Select(segment =>
                    new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                    {
                        SegmentId = segment.Key.ToInt32(),
                        ReusableAttributeIdentifiers = segment.Value.Select(lane => lane.ToInt32()).ToArray()
                    }).ToArray(),
                SegmentReusableWidthAttributeIdentifiers = _segmentReusableWidthAttributeIdentifiers.Select(
                    segment =>
                        new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                        {
                            SegmentId = segment.Key.ToInt32(),
                            ReusableAttributeIdentifiers = segment.Value.Select(width => width.ToInt32()).ToArray()
                        }).ToArray(),
                SegmentReusableSurfaceAttributeIdentifiers = _segmentReusableSurfaceAttributeIdentifiers.Select(
                    segment =>
                        new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                        {
                            SegmentId = segment.Key.ToInt32(),
                            ReusableAttributeIdentifiers =
                                segment.Value.Select(surface => surface.ToInt32()).ToArray()
                        }).ToArray()
            };
        }

        public IRoadNetworkView ToBuilder()
        {
            return this;
        }

        public IRoadNetworkView ToImmutable()
        {
            return new ImmutableRoadNetworkView(
                _nodes.ToImmutable(),
                _segments.ToImmutable(),
                _gradeSeparatedJunctions.ToImmutable(),
                _segmentReusableLaneAttributeIdentifiers.ToImmutable(),
                _segmentReusableWidthAttributeIdentifiers.ToImmutable(),
                _segmentReusableSurfaceAttributeIdentifiers.ToImmutable());
        }

        public IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            foreach (var change in changes)
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        With(addRoadNode);
                        break;
                    case ModifyRoadNode modifyRoadNode:
                        With(modifyRoadNode);
                        break;
                    case RemoveRoadNode removeRoadNode:
                        With(removeRoadNode);
                        break;
                    case AddRoadSegment addRoadSegment:
                        With(addRoadSegment);
                        break;
                    case ModifyRoadSegment modifyRoadSegment:
                        With(modifyRoadSegment);
                        break;
                    case ModifyRoadSegmentAttributes modifyRoadSegmentAttributes:
                        With(modifyRoadSegmentAttributes);
                        break;
                    case ModifyRoadSegmentGeometry modifyRoadSegmentGeometry:
                        With(modifyRoadSegmentGeometry);
                        break;
                    case RemoveRoadSegment removeRoadSegment:
                        With(removeRoadSegment);
                        break;
                    case RemoveRoadSegments removeRoadSegments:
                        With(removeRoadSegments);
                        break;
                    case AddRoadSegmentToEuropeanRoad addRoadSegmentToEuropeanRoad:
                        With(addRoadSegmentToEuropeanRoad);
                        break;
                    case RemoveRoadSegmentFromEuropeanRoad removeRoadSegmentFromEuropeanRoad:
                        With(removeRoadSegmentFromEuropeanRoad);
                        break;
                    case AddRoadSegmentToNationalRoad addRoadSegmentToNationalRoad:
                        With(addRoadSegmentToNationalRoad);
                        break;
                    case RemoveRoadSegmentFromNationalRoad removeRoadSegmentFromNationalRoad:
                        With(removeRoadSegmentFromNationalRoad);
                        break;
                    case AddRoadSegmentToNumberedRoad addRoadSegmentToNumberedRoad:
                        With(addRoadSegmentToNumberedRoad);
                        break;
                    case RemoveRoadSegmentFromNumberedRoad removeRoadSegmentFromNumberedRoad:
                        With(removeRoadSegmentFromNumberedRoad);
                        break;
                    case AddGradeSeparatedJunction addGradeSeparatedJunction:
                        With(addGradeSeparatedJunction);
                        break;
                    case ModifyGradeSeparatedJunction modifyGradeSeparatedJunction:
                        With(modifyGradeSeparatedJunction);
                        break;
                    case RemoveGradeSeparatedJunction removeGradeSeparatedJunction:
                        With(removeGradeSeparatedJunction);
                        break;
                }

            return this;
        }

        private void Given(ImportedRoadNode @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var id = new RoadNodeId(@event.Id);
            var version = new RoadNodeVersion(@event.Version);
            var type = RoadNodeType.Parse(@event.Type);
            var node = new RoadNode(id, version, type, GeometryTranslator.Translate(@event.Geometry));
            _nodes.Add(id, node);
        }

        private void Given(ImportedRoadSegment @event)
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
            {
                var attribute = new RoadSegmentEuropeanRoadAttribute(new AttributeId(europeanRoad.AttributeId), EuropeanRoadNumber.Parse(europeanRoad.Number));
                return current.PartOfEuropeanRoad(attribute);
            });
            segment = @event.PartOfNationalRoads.Aggregate(segment, (current, nationalRoad) =>
            {
                var attribute = new RoadSegmentNationalRoadAttribute(new AttributeId(nationalRoad.AttributeId), NationalRoadNumber.Parse(nationalRoad.Number));
                return current.PartOfNationalRoad(attribute);
            });
            segment = @event.PartOfNumberedRoads.Aggregate(segment, (current, numberedRoad) =>
            {
                var attribute = new RoadSegmentNumberedRoadAttribute(
                    new AttributeId(numberedRoad.AttributeId),
                    RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction),
                    NumberedRoadNumber.Parse(numberedRoad.Number),
                    new RoadSegmentNumberedRoadOrdinal(numberedRoad.Ordinal)
                );
                return current.PartOfNumberedRoad(attribute);
            });
            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id));
            _segments.Add(id, segment);
            _segmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId)));
            _segmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)));
        }

        private void Given(ImportedGradeSeparatedJunction @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var id = new GradeSeparatedJunctionId(@event.Id);
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            );
        }

        private void Given(RoadNetworkChangesAccepted @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            foreach (var change in @event.Changes.Flatten())
                switch (change)
                {
                    case RoadNodeAdded roadNodeAdded:
                        Given(roadNodeAdded);
                        break;
                    case RoadNodeModified roadNodeModified:
                        Given(roadNodeModified);
                        break;
                    case RoadNodeRemoved roadNodeRemoved:
                        Given(roadNodeRemoved);
                        break;
                    case RoadSegmentAdded roadSegmentAdded:
                        Given(roadSegmentAdded);
                        break;
                    case RoadSegmentModified roadSegmentModified:
                        Given(roadSegmentModified);
                        break;
                    case RoadSegmentAttributesModified roadSegmentAttributesModified:
                        Given(roadSegmentAttributesModified);
                        break;
                    case RoadSegmentGeometryModified roadSegmentGeometryModified:
                        Given(roadSegmentGeometryModified);
                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        Given(roadSegmentRemoved);
                        break;
                    case OutlinedRoadSegmentRemoved outlinedRoadSegmentRemoved:
                        Given(outlinedRoadSegmentRemoved);
                        break;
                    case RoadSegmentAddedToEuropeanRoad roadSegmentAddedToEuropeanRoad:
                        Given(roadSegmentAddedToEuropeanRoad);
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad roadSegmentRemovedFromEuropeanRoad:
                        Given(roadSegmentRemovedFromEuropeanRoad);
                        break;
                    case RoadSegmentAddedToNationalRoad roadSegmentAddedToNationalRoad:
                        Given(roadSegmentAddedToNationalRoad);
                        break;
                    case RoadSegmentRemovedFromNationalRoad roadSegmentRemovedFromNationalRoad:
                        Given(roadSegmentRemovedFromNationalRoad);
                        break;
                    case RoadSegmentAddedToNumberedRoad roadSegmentAddedToNumberedRoad:
                        Given(roadSegmentAddedToNumberedRoad);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad roadSegmentRemovedFromNumberedRoad:
                        Given(roadSegmentRemovedFromNumberedRoad);
                        break;
                    case GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
                        Given(gradeSeparatedJunctionAdded);
                        break;
                    case GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
                        Given(gradeSeparatedJunctionModified);
                        break;
                    case GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved:
                        Given(gradeSeparatedJunctionRemoved);
                        break;
                }
        }

        private void Given(RoadNodeAdded @event)
        {
            var id = new RoadNodeId(@event.Id);
            var version = new RoadNodeVersion(@event.Version);
            var type = RoadNodeType.Parse(@event.Type);
            var node = new RoadNode(id, version, type, GeometryTranslator.Translate(@event.Geometry));
            _nodes.Add(id, node);
        }

        private void Given(RoadNodeModified @event)
        {
            var id = new RoadNodeId(@event.Id);
            _nodes.TryReplace(id, node => node
                .WithType(RoadNodeType.Parse(@event.Type))
                .WithGeometry(GeometryTranslator.Translate(@event.Geometry))
                .WithVersion(new RoadNodeVersion(@event.Version))
            );
        }

        private void Given(RoadNodeRemoved @event)
        {
            var id = new RoadNodeId(@event.Id);
            _nodes.Remove(id);
        }

        private void Given(RoadSegmentAdded @event)
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

            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id));
            _segments.Add(id, segment);
            _segmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId)));
            _segmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)));
        }

        private void Given(RoadSegmentModified @event)
        {
            var id = new RoadSegmentId(@event.Id);

            if (@event.ConvertedFromOutlined && !_segments.ContainsKey(id))
            {
                Given(new RoadSegmentAdded
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
                    Widths = @event.Widths,
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

            var segmentBefore = _segments[id];

            _nodes
                .TryReplaceIf(segmentBefore.Start, node => node.Id != start, node => node.DisconnectFrom(id))
                .TryReplaceIf(segmentBefore.End, node => node.Id != end, node => node.DisconnectFrom(id))
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id));
            _segments
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
                );
            _segmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId)));
            _segmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)));
        }

        private void Given(RoadSegmentAttributesModified @event)
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
                );
        }

        private void Given(RoadSegmentGeometryModified @event)
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
                );
            _segmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId)));
            _segmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)));
        }

        private void Given(RoadSegmentRemoved @event)
        {
            var id = new RoadSegmentId(@event.Id);
            if (_segments.TryGetValue(id, out var segment))
            {
                _nodes.TryReplace(segment.Start, node => node.DisconnectFrom(id));
                _nodes.TryReplace(segment.End, node => node.DisconnectFrom(id));
            }

            _segments.Remove(id);
        }

        private void Given(OutlinedRoadSegmentRemoved @event)
        {
            var id = new RoadSegmentId(@event.Id);
            if (_segments.TryGetValue(id, out var segment) && segment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
            {
                _segments.Remove(id);
            }
        }

        private void Given(GradeSeparatedJunctionAdded @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            );
        }

        private void Given(GradeSeparatedJunctionModified @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            _gradeSeparatedJunctions.TryReplace(
                id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(GradeSeparatedJunctionType.Parse(@event.Type))
                        .WithUpperSegment(new RoadSegmentId(@event.UpperRoadSegmentId))
                        .WithLowerSegment(new RoadSegmentId(@event.LowerRoadSegmentId))
            );
        }

        private void Given(GradeSeparatedJunctionRemoved @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            _gradeSeparatedJunctions.Remove(id);
        }

        private void Given(RoadSegmentAddedToEuropeanRoad @event)
        {
            var attributeId = new AttributeId(@event.AttributeId);
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var europeanRoadNumber = EuropeanRoadNumber.Parse(@event.Number);
            var roadSegmentVersion = RoadSegmentVersion.FromValue(@event.SegmentVersion);

            var attribute = new RoadSegmentEuropeanRoadAttribute(attributeId, europeanRoadNumber);

            _segments.TryReplace(roadSegmentId, segment => segment
                .PartOfEuropeanRoad(attribute)
                .WithVersion(roadSegmentVersion ?? segment.Version));
        }

        private void Given(RoadSegmentRemovedFromEuropeanRoad @event)
        {
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var roadSegmentVersion = RoadSegmentVersion.FromValue(@event.SegmentVersion);

            _segments.TryReplace(roadSegmentId, segment => segment
                .NotPartOfEuropeanRoad(new AttributeId(@event.AttributeId))
                .WithVersion(roadSegmentVersion ?? segment.Version));
        }

        private void Given(RoadSegmentAddedToNationalRoad @event)
        {
            var attributeId = new AttributeId(@event.AttributeId);
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var nationalRoadNumber = NationalRoadNumber.Parse(@event.Number);
            var roadSegmentVersion = RoadSegmentVersion.FromValue(@event.SegmentVersion);

            var attribute = new RoadSegmentNationalRoadAttribute(attributeId, nationalRoadNumber);

            _segments.TryReplace(roadSegmentId, segment => segment
                .PartOfNationalRoad(attribute)
                .WithVersion(roadSegmentVersion ?? segment.Version));
        }

        private void Given(RoadSegmentRemovedFromNationalRoad @event)
        {
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var roadSegmentVersion = RoadSegmentVersion.FromValue(@event.SegmentVersion);

            _segments.TryReplace(roadSegmentId, segment => segment
                .NotPartOfNationalRoad(new AttributeId(@event.AttributeId))
                .WithVersion(roadSegmentVersion ?? segment.Version));
        }

        private void Given(RoadSegmentAddedToNumberedRoad @event)
        {
            var attributeId = new AttributeId(@event.AttributeId);
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var numberedRoadNumber = NumberedRoadNumber.Parse(@event.Number);
            var roadSegmentVersion = RoadSegmentVersion.FromValue(@event.SegmentVersion);

            var attribute = new RoadSegmentNumberedRoadAttribute(
                attributeId,
                RoadSegmentNumberedRoadDirection.Parse(@event.Direction),
                numberedRoadNumber,
                new RoadSegmentNumberedRoadOrdinal(@event.Ordinal)
            );

            _segments.TryReplace(roadSegmentId, segment => segment
                .PartOfNumberedRoad(attribute)
                .WithVersion(roadSegmentVersion ?? segment.Version));
        }

        private void Given(RoadSegmentRemovedFromNumberedRoad @event)
        {
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var roadSegmentVersion = RoadSegmentVersion.FromValue(@event.SegmentVersion);

            _segments.TryReplace(roadSegmentId, segment => segment
                .NotPartOfNumberedRoad(new AttributeId(@event.AttributeId))
                .WithVersion(roadSegmentVersion ?? segment.Version));
        }

        private void With(AddRoadNode command)
        {
            var version = RoadNodeVersion.Initial;
            var node = new RoadNode(command.Id, version, command.Type, command.Geometry);
            _nodes.Add(command.Id, node);
        }

        private void With(ModifyRoadNode command)
        {
            _nodes[command.Id] = _nodes[command.Id].WithGeometry(command.Geometry).WithType(command.Type);
        }

        private void With(RemoveRoadNode command)
        {
            _nodes.Remove(command.Id);
        }

        private void With(AddRoadSegment command)
        {
            var version = RoadSegmentVersion.Initial;
            var geometryVersion = GeometryVersion.Initial;

            var attributeHash = new AttributeHash(
                command.AccessRestriction,
                command.Category,
                command.Morphology,
                command.Status,
                command.LeftSideStreetNameId,
                command.RightSideStreetNameId,
                command.MaintenanceAuthorityId,
                command.GeometryDrawMethod
            );

            _nodes
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id));
            _segments.Add(command.Id,
                new RoadSegment(command.Id, version, command.Geometry, geometryVersion, command.StartNodeId, command.EndNodeId,
                    attributeHash,
                    command.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                        lane.From,
                        lane.To,
                        lane.Count,
                        lane.Direction,
                        lane.AsOfGeometryVersion)).ToArray(),
                    command.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                        surface.From,
                        surface.To,
                        surface.Type,
                        surface.AsOfGeometryVersion)).ToArray(),
                    command.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                        width.From,
                        width.To,
                        width.Width,
                        width.AsOfGeometryVersion)).ToArray(), command.GetHash()));
            _segmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id)));
            _segmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)));
        }

        private void With(ModifyRoadSegment command)
        {
            if (command.ConvertedFromOutlined && !_segments.ContainsKey(command.Id))
            {
                With(new AddRoadSegment(
                    command.Id,
                    command.Id,
                    command.OriginalId ?? command.Id,
                    command.Id,
                    command.StartNodeId,
                    command.EndNodeId,
                    command.EndNodeId,
                    command.TemporaryEndNodeId,
                    command.Geometry,
                    command.MaintenanceAuthorityId,
                    command.MaintenanceAuthorityName,
                    command.GeometryDrawMethod,
                    command.Morphology,
                    command.Status,
                    command.Category,
                    command.AccessRestriction,
                    command.LeftSideStreetNameId,
                    command.RightSideStreetNameId,
                    command.Lanes,
                    command.Widths,
                    command.Surfaces
                ));
            }

            var attributeHash = new AttributeHash(
                command.AccessRestriction,
                command.Category,
                command.Morphology,
                command.Status,
                command.LeftSideStreetNameId,
                command.RightSideStreetNameId,
                command.MaintenanceAuthorityId,
                command.GeometryDrawMethod
            );

            var segmentBefore = _segments[command.Id];

            _nodes
                .TryReplaceIf(segmentBefore.Start, node => node.Id != command.StartNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplaceIf(segmentBefore.End, node => node.Id != command.EndNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id));
            _segments
                .TryReplace(command.Id, segment => segment
                    .WithVersion(command.Version)
                    .WithGeometry(command.Geometry)
                    .WithGeometryVersion(command.GeometryVersion)
                    .WithStartAndEndAndAttributeHash(command.StartNodeId, command.EndNodeId, attributeHash)
                    .WithLanes(command.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                        lane.From,
                        lane.To,
                        lane.Count,
                        lane.Direction,
                        lane.AsOfGeometryVersion)).ToArray())
                    .WithSurfaces(command.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                        surface.From,
                        surface.To,
                        surface.Type,
                        surface.AsOfGeometryVersion)).ToArray())
                    .WithWidths(command.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                        width.From,
                        width.To,
                        width.Width,
                        width.AsOfGeometryVersion)).ToArray())
                    .WithLastEventHash(command.GetHash())
                );
            _segmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id)));
            _segmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)));
        }

        private void With(ModifyRoadSegmentAttributes command)
        {
            //NOTE: 2023-03-31 Rik: need to test, no idea how this code ever gets executed
            var segmentBefore = _segments[command.Id];

            var attributeHash = new AttributeHash(
                command.AccessRestriction ?? segmentBefore.AttributeHash.AccessRestriction,
                command.Category ?? segmentBefore.AttributeHash.Category,
                command.Morphology ?? segmentBefore.AttributeHash.Morphology,
                command.Status ?? segmentBefore.AttributeHash.Status,
                command.LeftSide is not null
                    ? command.LeftSide.StreetNameId
                    : segmentBefore.AttributeHash.LeftStreetNameId,
                command.RightSide is not null
                    ? command.RightSide.StreetNameId
                    : segmentBefore.AttributeHash.RightStreetNameId,
                command.MaintenanceAuthorityId ?? segmentBefore.AttributeHash.OrganizationId,
                command.GeometryDrawMethod
            );

            _segments
                .TryReplace(command.Id, segment => segment
                    .WithVersion(command.Version)
                    .WithAttributeHash(attributeHash)
                    .WithLanes(command.Lanes?
                        .Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                            lane.From,
                            lane.To,
                            lane.Count,
                            lane.Direction,
                            lane.AsOfGeometryVersion))
                        .ToArray() ?? segmentBefore.Lanes)
                    .WithSurfaces(command.Surfaces?
                        .Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                            surface.From,
                            surface.To,
                            surface.Type,
                            surface.AsOfGeometryVersion))
                        .ToArray() ?? segmentBefore.Surfaces)
                    .WithWidths(command.Widths?
                        .Select(width => new BackOffice.RoadSegmentWidthAttribute(
                            width.From,
                            width.To,
                            width.Width,
                            width.AsOfGeometryVersion))
                        .ToArray() ?? segmentBefore.Widths)
                    .WithLastEventHash(command.GetHash())
                );
        }

        private void With(ModifyRoadSegmentGeometry command)
        {
            //NOTE: 2023-03-31 Rik: need to test, no idea how this code ever gets executed
            var segmentBefore = _segments[command.Id];

            var attributeHash = new AttributeHash(
                segmentBefore.AttributeHash.AccessRestriction,
                segmentBefore.AttributeHash.Category,
                segmentBefore.AttributeHash.Morphology,
                segmentBefore.AttributeHash.Status,
                segmentBefore.AttributeHash.LeftStreetNameId,
                segmentBefore.AttributeHash.RightStreetNameId,
                segmentBefore.AttributeHash.OrganizationId,
                command.GeometryDrawMethod
            );

            var geometry = command.Geometry;
            var geometryVersion = command.GeometryVersion;

            _segments
                .TryReplace(command.Id, segment => segment
                    .WithVersion(command.Version)
                    .WithGeometryVersion(geometryVersion)
                    .WithGeometry(geometry)
                    .WithAttributeHash(attributeHash)
                    .WithLanes(command.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                        lane.From,
                        lane.To,
                        lane.Count,
                        lane.Direction,
                        lane.AsOfGeometryVersion)).ToArray())
                    .WithSurfaces(command.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                        surface.From,
                        surface.To,
                        surface.Type,
                        surface.AsOfGeometryVersion)).ToArray())
                    .WithWidths(command.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                        width.From,
                        width.To,
                        width.Width,
                        width.AsOfGeometryVersion)).ToArray())
                    .WithLastEventHash(command.GetHash())
                );
            _segmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id)));
            _segmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)));
        }

        private void With(RemoveRoadSegment command)
        {
            if (_segments.TryGetValue(command.Id, out var segment))
            {
                _nodes.TryReplace(segment.Start, node => node.DisconnectFrom(command.Id));
                _nodes.TryReplace(segment.End, node => node.DisconnectFrom(command.Id));
            }

            _segments.Remove(command.Id);
        }

        private void With(RemoveRoadSegments command)
        {
            var segments = _segments;
            var nodes = _nodes;
            var junctions = _gradeSeparatedJunctions;

            foreach (var roadSegmentId in command.Ids)
            {
                segments.TryGetValue(roadSegmentId, out var segment);
                segments.Remove(roadSegmentId);

                if (segment!.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
                {
                    continue;
                }

                var linkedJunctions = junctions
                    .Where(x => x.Value.LowerSegment == roadSegmentId || x.Value.UpperSegment == roadSegmentId)
                    .Select(x => x.Value)
                    .ToList();
                foreach (var junction in linkedJunctions)
                {
                    junctions
                        .Remove(junction.Id);
                }

                DisconnectAndUpdateNode(segment.Start);
                DisconnectAndUpdateNode(segment.End);
                continue;

                //TODO-pr ook implementeren

                void DisconnectAndUpdateNode(RoadNodeId nodeId)
                {
                    nodes = nodes.TryReplace(nodeId, x => x.DisconnectFrom(roadSegmentId));

                    nodes.TryGetValue(nodeId, out var node);
                    if (node!.Type == RoadNodeType.EndNode)
                    {
                        nodes.Remove(nodeId);
                    }
                }
            }
        }

        private void With(AddGradeSeparatedJunction command)
        {
            _gradeSeparatedJunctions.Add(
                command.Id,
                new GradeSeparatedJunction(command.Id, command.Type, command.UpperSegmentId, command.LowerSegmentId));
        }

        private void With(ModifyGradeSeparatedJunction command)
        {
            _gradeSeparatedJunctions.TryReplace(
                command.Id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(command.Type)
                        .WithUpperSegment(command.UpperSegmentId)
                        .WithLowerSegment(command.LowerSegmentId));
        }

        private void With(RemoveGradeSeparatedJunction command)
        {
            _gradeSeparatedJunctions.Remove(command.Id);
        }

        private void With(AddRoadSegmentToEuropeanRoad command)
        {
            _segments.TryReplace(command.SegmentId, segment => segment
                .WithVersion(command.SegmentVersion)
                .WithLastEventHash(command.GetHash()));
        }

        private void With(RemoveRoadSegmentFromEuropeanRoad command)
        {
            _segments.TryReplace(command.SegmentId, segment => segment
                .WithVersion(command.SegmentVersion)
                .WithLastEventHash(command.GetHash()));
        }

        private void With(AddRoadSegmentToNationalRoad command)
        {
            _segments.TryReplace(command.SegmentId, segment => segment
                .WithVersion(command.SegmentVersion)
                .WithLastEventHash(command.GetHash()));
        }

        private void With(RemoveRoadSegmentFromNationalRoad command)
        {
            _segments.TryReplace(command.SegmentId, segment => segment
                .WithVersion(command.SegmentVersion)
                .WithLastEventHash(command.GetHash()));
        }

        private void With(AddRoadSegmentToNumberedRoad command)
        {
            _segments.TryReplace(command.SegmentId, segment => segment
                .WithVersion(command.SegmentVersion)
                .WithLastEventHash(command.GetHash()));
        }

        private void With(RemoveRoadSegmentFromNumberedRoad command)
        {
            _segments.TryReplace(command.SegmentId, segment => segment
                .WithVersion(command.SegmentVersion)
                .WithLastEventHash(command.GetHash()));
        }
    }
}
