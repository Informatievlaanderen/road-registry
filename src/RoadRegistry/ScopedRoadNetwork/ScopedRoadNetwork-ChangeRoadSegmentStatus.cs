namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // Every road segment status change runs through here. 'gerealiseerd' is the only status that knots a segment into
    // the network, so which of the three shapes below applies follows entirely from the two statuses - see
    // RoadSegmentStatusChange.
    //
    // The request carries no body in any of them: what happens follows entirely from the network around the segment.
    public RoadNetworkChangeResult ChangeRoadSegmentStatus(
        RoadSegmentStatusChange statusChange,
        RoadSegmentId roadSegmentId,
        bool mayModifyMeasuredRoadSegments,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        // Everything this action can go wrong about is about the one segment it names, but the mutations further down
        // raise problems that identify themselves, so the context is handed to each error rather than carried.
        var roadSegmentContext = Problems.WithContext(roadSegmentId);

        // VAL-2, VAL-3
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return Failed(roadSegmentContext + new RoadSegmentNotFound(), context);
        }

        var problems = ValidateRoadSegmentIsChangeable(roadSegment, statusChange, mayModifyMeasuredRoadSegments, roadSegmentContext);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        if (statusChange.Connects)
        {
            return ConnectRoadSegment(statusChange, roadSegment, mayModifyMeasuredRoadSegments, idGenerator, provenance, context, roadSegmentContext, logger);
        }

        if (statusChange.Disconnects)
        {
            return DisconnectRoadSegment(statusChange, roadSegment, idGenerator, provenance, context, logger);
        }

        return ChangeUnconnectedRoadSegmentStatus(statusChange, roadSegment, provenance, context);
    }

    private static Problems ValidateRoadSegmentIsChangeable(
        RoadSegment roadSegment,
        RoadSegmentStatusChange statusChange,
        bool mayModifyMeasuredRoadSegments,
        Problems roadSegmentContext)
    {
        var problems = Problems.None;

        // Re-validate what the API also checks: the request may have gone stale between being accepted and handled.
        if (!roadSegment.HasMigrated())
        {
            problems += roadSegmentContext + new RoadSegmentNotCompletedInwinning();
        }
        // VAL-4
        if (roadSegment.Status != statusChange.From)
        {
            problems += roadSegmentContext + new RoadSegmentStatusNotValidForStatusChange(statusChange.StatusNotValidProblemCode, statusChange.From);
        }
        // Only a holder of the 'ingemeten' scope may change the status of a measured road segment.
        if (!mayModifyMeasuredRoadSegments && roadSegment.Attributes?.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten)
        {
            problems += roadSegmentContext + new RoadSegmentMeasuredNotAllowed();
        }

        return problems;
    }

    // 'Knoop een wegsegment aan het wegennet' - the shape of 'markeer een gepland wegsegment als gerealiseerd' and of
    // every other change into 'gerealiseerd'.
    //
    // A segment that is not realized is drawn but not knotted into the network: it carries no road nodes. Realizing it
    // hooks it up. Each endpoint either snaps onto an existing road node within reach - the geometry follows, the node
    // keeps its identifier - or gets an 'eindknoop' of its own where it lies. At least one endpoint has to find an
    // existing node, otherwise the segment would be an island.
    //
    // The user is expected to have prepared the connection points by splitting beforehand: an endpoint that lands near
    // an existing *segment* rather than an existing node gets its own end node, however close that segment is.
    private RoadNetworkChangeResult ConnectRoadSegment(
        RoadSegmentStatusChange statusChange,
        RoadSegment roadSegment,
        bool mayModifyMeasuredRoadSegments,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ScopedRoadNetworkChangeContext context,
        Problems roadSegmentContext,
        ILogger logger)
    {
        var roadSegmentId = roadSegment.RoadSegmentId;

        // VAL-7, VAL-8: on the geometry as drawn, before anything is snapped. The translation of some of these looks
        // the road segment up, so they go under its context too.
        var problems = roadSegmentContext + roadSegment.Geometry.ValidateRoadSegmentGeometryDomainV2();
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        RebuildSpatialIndexes(logger);

        var line = roadSegment.Geometry.Value.GetSingleLineString();
        var startNode = FindRoadNodeInReachOf(line.Coordinates[0]);
        var endNode = FindRoadNodeInReachOf(line.Coordinates[^1]);

        // VAL-5
        if (startNode is null && endNode is null)
        {
            return Failed(
                roadSegmentContext + new RoadSegmentRealizeNoRoadNodeInReach(Distances.RoadSegmentRealizeMaximumDistanceToRoadNode),
                context);
        }

        // A road node that has not completed its inwinning is still a V1 node: it carries no type, and it sits on the
        // coordinate it was imported with, which is more precise than the centimetre this register works in. Snapping
        // onto it would either move the segment off the centimetre grid or, once the geometry is rounded, off the node
        // itself - and the segment would then fail to resolve the very node it snapped to. It is not ours to knot onto
        // until it has been migrated.
        foreach (var roadNode in new[] { startNode, endNode }.Where(x => x is not null && !x.HasMigrated()))
        {
            problems += new RoadNodeNotCompletedInwinning(roadNode!.RoadNodeId);
        }
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        // Snapping replaces the endpoint vertex with the node it snapped to; every other vertex stays where it is.
        var snappedGeometry = SnapEndpointsTo(roadSegment.Geometry, startNode, endNode);

        // An endpoint that found nothing terminates the road, so it gets an end node of its own. It is placed on the
        // snapped geometry, which at that end is the endpoint as drawn.
        var snappedLine = snappedGeometry.Value.GetSingleLineString();
        if (startNode is null)
        {
            problems += AddEndRoadNodeAt(snappedLine.Coordinates[0], snappedGeometry.SRID, idGenerator, context);
        }
        if (endNode is null)
        {
            problems += AddEndRoadNodeAt(snappedLine.Coordinates[^1], snappedGeometry.SRID, idGenerator, context);
        }
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        // Snapping shortens or lengthens the stretch between the endpoint and the vertex next to it, so the attributes
        // are remapped against where every vertex sits before and after.
        var currentVertexPositions = CumulativeVertexPositions(line);
        var newVertexPositions = CumulativeVertexPositions(snappedLine);
        var attributes = roadSegment.Attributes!;

        var realizedAttributes = attributes with
        {
            AccessRestriction = attributes.AccessRestriction.RemapTo(currentVertexPositions, newVertexPositions),
            Category = attributes.Category.RemapTo(currentVertexPositions, newVertexPositions),
            Morphology = attributes.Morphology.RemapTo(currentVertexPositions, newVertexPositions),
            StreetNameId = attributes.StreetNameId.RemapTo(currentVertexPositions, newVertexPositions),
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId.RemapTo(currentVertexPositions, newVertexPositions),
            SurfaceType = attributes.SurfaceType.RemapTo(currentVertexPositions, newVertexPositions),
            CarTrafficDirection = attributes.CarTrafficDirection.RemapTo(currentVertexPositions, newVertexPositions),
            BikeTrafficDirection = attributes.BikeTrafficDirection.RemapTo(currentVertexPositions, newVertexPositions),
            PedestrianTrafficDirection = attributes.PedestrianTrafficDirection.RemapTo(currentVertexPositions, newVertexPositions)
        };

        // Realizing is its own action rather than a modification: it records what the segment became in one event.
        // The start and end node are resolved from whatever sits on the endpoints, so both the snapped-onto nodes and
        // the ones just added are in place by now.
        var oldEnvelope = roadSegment.Geometry.Value.EnvelopeInternal;
        problems += roadSegment.ChangeStatusToConnected(statusChange, snappedGeometry, realizedAttributes, context);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        _roadSegmentsSpatialIndex.Update(oldEnvelope, roadSegment.Geometry.Value.EnvelopeInternal, roadSegment);
        context.Summary.RoadSegments.Modified.Add(roadSegmentId);

        // The node the segment snapped onto now carries one segment more than it did, so its type is re-derived. This
        // is what turns the 'validatieknoop' left behind by a split into an 'echte knoop'.
        //
        // Merging is off: snapping onto the 'eindknoop' of an existing road leaves that node with two segments, and
        // the network-wide rules would take that as licence to merge the two into one. Realizing a segment is an edit
        // of the one segment it names - the road it hooks onto keeps its own identity.
        problems += VerifyRoadNodesTopologyAndUpdateTypeAfterChange(idGenerator, context, mayMergeRoadSegments: false);

        // VAL-6, and VAL-7/VAL-8 once more now that the segment counts as realized.
        problems += VerifyRoadSegmentsTopologyAfterChange(context);

        // A crossing this segment makes with another realized segment becomes a gelijkgrondse kruising, whether or not
        // the two share a traffic type - a suspicious crossing is reported as such rather than refused here.
        if (!problems.HasError())
        {
            problems += VerifyAndUpdateJunctionsAfterGeometryChange(idGenerator, context);
        }

        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    // 'Maak een wegsegment los van het wegennet' - the shape of 'corrigeer een gerealiseerd wegsegment naar gepland'
    // and of every other change out of 'gerealiseerd'.
    //
    // The mirror image of connecting: only a realized segment is knotted into the network, so leaving 'gerealiseerd'
    // unhooks it. It gives up its two road nodes, the crossings it took part in go with it, and each node it hung off
    // is either removed - nothing else was using it - or re-typed for what is left.
    //
    // The geometry and every attribute stay exactly as they are: the segment is still drawn where it was drawn, it
    // simply is not part of the network any more.
    private RoadNetworkChangeResult DisconnectRoadSegment(
        RoadSegmentStatusChange statusChange,
        RoadSegment roadSegment,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ScopedRoadNetworkChangeContext context,
        ILogger logger)
    {
        var roadSegmentId = roadSegment.RoadSegmentId;

        RebuildSpatialIndexes(logger);

        // Held on to before the segment lets go of them: once it is unhooked it no longer says which nodes it hung off.
        var previousRoadNodeIds = roadSegment.GetNodeIds().ToArray();

        var problems = roadSegment.ChangeStatusFromConnected(statusChange, context);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }
        context.Summary.RoadSegments.Modified.Add(roadSegmentId);

        // A crossing is a statement about two realized roads. This one is not realized any more, so whatever recorded
        // its crossings goes with it - grade and grade separated alike.
        problems += TryToRemoveLinkedGradeJunctions(roadSegmentId, context);
        problems += TryToRemoveLinkedGradeSeparatedJunctions(roadSegmentId, context);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        problems += UnhookRoadNodes(previousRoadNodeIds, idGenerator, context);

        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    // A status change between two statuses that both leave the segment outside the network. It carries no road nodes
    // before and none after, takes part in no crossings, and its geometry and attributes are untouched - so there is
    // no topology to work out at all and nothing but the segment itself changes.
    private RoadNetworkChangeResult ChangeUnconnectedRoadSegmentStatus(
        RoadSegmentStatusChange statusChange,
        RoadSegment roadSegment,
        Provenance provenance,
        ScopedRoadNetworkChangeContext context)
    {
        var problems = roadSegment.ChangeStatusWhileUnconnected(statusChange, context);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        context.Summary.RoadSegments.Modified.Add(roadSegment.RoadSegmentId);

        if (context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    // The nearest non-removed road node within reach of a point, or nothing. Ties go to whichever the index returned
    // first, which is what the analysis asks for.
    private RoadNode.RoadNode? FindRoadNodeInReachOf(Coordinate coordinate)
    {
        var maximumDistance = Distances.RoadSegmentRealizeMaximumDistanceToRoadNode;

        var envelope = new Envelope(coordinate);
        envelope.ExpandBy(maximumDistance);

        return _roadNodesSpatialIndex
            .Query(envelope)
            .Where(x => !x.IsRemoved)
            .Select(x => (RoadNode: x, Distance: x.Geometry.Value.Coordinate.Distance(coordinate)))
            .Where(x => x.Distance <= maximumDistance)
            .OrderBy(x => x.Distance)
            .Select(x => x.RoadNode)
            .FirstOrDefault();
    }

    private static RoadSegmentGeometry SnapEndpointsTo(RoadSegmentGeometry geometry, RoadNode.RoadNode? startNode, RoadNode.RoadNode? endNode)
    {
        var line = geometry.Value.GetSingleLineString();
        var coordinates = line.Coordinates.ToArray();

        if (startNode is not null)
        {
            coordinates[0] = startNode.Geometry.Value.Coordinate.Copy();
        }
        if (endNode is not null)
        {
            coordinates[^1] = endNode.Geometry.Value.Coordinate.Copy();
        }

        return new MultiLineString([line.Factory.CreateLineString(coordinates)])
            .WithSrid(geometry.SRID)
            .ToRoadSegmentGeometry()
            .RoundToCm();
    }

    private Problems AddEndRoadNodeAt(Coordinate coordinate, int srid, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        // The type is what this endpoint is on its own; the node verification further down re-derives it once the
        // segment hangs off it.
        return AddRoadNode(new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(context.Summary.RoadNodes.Added.Count + 1),
            Geometry = RoadNodeGeometry.Create(new Point(coordinate.Copy()).WithSrid(srid)),
            Grensknoop = false,
            Type = RoadNodeTypeV2.Eindknoop
        }, idGenerator, context);
    }

    // A node the segment came loose from is either not holding anything up any more, in which case it goes, or still
    // carries other segments and is re-typed for what is left of them.
    private Problems UnhookRoadNodes(IReadOnlyCollection<RoadNodeId> roadNodeIds, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        foreach (var roadNodeId in roadNodeIds)
        {
            if (!_roadNodes.TryGetValue(roadNodeId, out var roadNode) || roadNode.IsRemoved)
            {
                continue;
            }

            var remainingSegments = GetNonRemovedRoadSegments()
                .Count(x => x.StartNodeId == roadNodeId || x.EndNodeId == roadNodeId);
            if (remainingSegments == 0)
            {
                problems += RemoveRoadNode(roadNodeId, context);
                continue;
            }

            // Merging is off for the same reason as when connecting: this action names one segment, and the roads that
            // happened to meet it keep their own identity.
            problems += roadNode.VerifyTopologyAndUpdateType(_roadSegmentsSpatialIndex, idGenerator, context, mayMergeRoadSegments: false);
            if (roadNode.HasChanges())
            {
                context.Summary.RoadNodes.Modified.Add(roadNodeId);
            }
        }

        return problems;
    }
}
