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
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // 'Markeer een gepland wegsegment als gerealiseerd'.
    //
    // A 'gepland' segment is drawn but not knotted into the network: it carries no road nodes. Realizing it hooks it
    // up. Each endpoint either snaps onto an existing road node within reach - the geometry follows, the node keeps
    // its identifier - or gets an 'eindknoop' of its own where it lies. At least one endpoint has to find an existing
    // node, otherwise the segment would be an island.
    //
    // The user is expected to have prepared the connection points by splitting beforehand: an endpoint that lands near
    // an existing *segment* rather than an existing node gets its own end node, however close that segment is.
    public RoadNetworkChangeResult RealizeRoadSegment(
        RealizeRoadSegmentChange change,
        bool mayModifyMeasuredRoadSegments,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        var roadSegmentId = change.RoadSegmentId;

        // Everything this action can go wrong about is about the one segment it names, but the mutations further down
        // raise problems that identify themselves, so the context is handed to each error rather than carried.
        var problems = Problems.None;
        var roadSegmentContext = Problems.WithContext(roadSegmentId);

        // VAL-2, VAL-3
        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return Failed(roadSegmentContext + new RoadSegmentNotFound(), context);
        }

        // Re-validate what the API also checks: the request may have gone stale between being accepted and handled.
        if (!roadSegment.HasMigrated())
        {
            problems += roadSegmentContext + new RoadSegmentNotCompletedInwinning();
        }
        // VAL-4
        if (roadSegment.Status != RoadSegmentStatusV2.Gepland)
        {
            problems += roadSegmentContext + new RoadSegmentRealizeStatusNotValid();
        }
        // VAL-9
        if (!mayModifyMeasuredRoadSegments && roadSegment.Attributes?.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten)
        {
            problems += roadSegmentContext + new RoadSegmentRealizeMeasuredNotAllowed();
        }
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        // VAL-7, VAL-8: on the geometry as drawn, before anything is snapped. The translation of some of these looks
        // the road segment up, so they go under its context too.
        problems += roadSegmentContext + roadSegment.Geometry.ValidateRoadSegmentGeometryDomainV2();
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

        // Modifying the segment resolves its start and end node from whatever sits on its endpoints, so both the
        // snapped-onto nodes and the ones just added are in place by now.
        problems += ModifyRoadSegment(new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
            Geometry = snappedGeometry,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            AccessRestriction = attributes.AccessRestriction.RemapTo(currentVertexPositions, newVertexPositions),
            Category = attributes.Category.RemapTo(currentVertexPositions, newVertexPositions),
            Morphology = attributes.Morphology.RemapTo(currentVertexPositions, newVertexPositions),
            StreetNameId = attributes.StreetNameId.RemapTo(currentVertexPositions, newVertexPositions),
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId.RemapTo(currentVertexPositions, newVertexPositions),
            SurfaceType = attributes.SurfaceType.RemapTo(currentVertexPositions, newVertexPositions),
            CarTrafficDirection = attributes.CarTrafficDirection.RemapTo(currentVertexPositions, newVertexPositions),
            BikeTrafficDirection = attributes.BikeTrafficDirection.RemapTo(currentVertexPositions, newVertexPositions),
            PedestrianTrafficDirection = attributes.PedestrianTrafficDirection.RemapTo(currentVertexPositions, newVertexPositions)
        }, context);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        // The node the segment snapped onto now carries one segment more than it did, so its type is re-derived. This
        // is what turns the 'validatieknoop' left behind by a split into an 'echte knoop'.
        problems += VerifyRoadNodesTopologyAndUpdateTypeAfterChange(idGenerator, context);

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
}
