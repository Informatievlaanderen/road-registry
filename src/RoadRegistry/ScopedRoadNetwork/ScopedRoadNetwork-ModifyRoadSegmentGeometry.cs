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
    // VAL-4: the geometry may only be changed on a road segment with one of these statuses.
    private static readonly RoadSegmentStatusV2[] ChangeGeometryAllowedStatuses =
    [
        RoadSegmentStatusV2.Gepland,
        RoadSegmentStatusV2.Gerealiseerd,
        RoadSegmentStatusV2.BuitenGebruik
    ];

    // 'Wijzig de geometrie van een wegsegment'.
    //
    // Road nodes are sticky: moving the start or end vertex of a segment moves the road node that sits on it, and
    // because that node moves, the corresponding vertex of every other segment connected to it moves along. The node
    // keeps its identifier throughout - it is modified, never replaced.
    //
    // The topology of the network is deliberately left alone: no node is added, none is removed, and every node keeps
    // exactly the segments it already had. That is why the road node type verification (which merges segments away
    // when a node turns out to be unnecessary) is not run here, and why a moved endpoint has to stay clear of the
    // other nodes rather than being allowed to snap onto one.
    public RoadNetworkChangeResult ModifyRoadSegmentGeometry(
        ModifyRoadSegmentGeometryChange change,
        bool mayModifyMeasuredRoadSegments,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        var roadSegmentId = change.RoadSegmentId;

        // Not carried on the accumulator: the problems raised once the mutations start come from the connected
        // segments too and already identify themselves. Each error below that is about the requested segment gets its
        // context handed to it instead.
        var problems = Problems.None;
        var roadSegmentContext = Problems.WithContext(roadSegmentId);

        if (!_roadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return Failed(roadSegmentContext + new RoadSegmentNotFound(), context);
        }

        // Re-validate what the API also checks: the request may have gone stale between being accepted and handled.
        if (!roadSegment.HasMigrated())
        {
            problems += roadSegmentContext + new RoadSegmentNotCompletedInwinning();
        }
        if (!ChangeGeometryAllowedStatuses.Contains(roadSegment.Status))
        {
            problems += roadSegmentContext + new RoadSegmentChangeGeometryStatusNotValid();
        }
        problems += ValidateGeometryDrawMethodIsEditable(roadSegment, mayModifyMeasuredRoadSegments);
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        var newGeometry = change.Geometry.RoundToCm();

        // Under the segment's context as well: these come back without an identifier, and the translation of some of
        // them (start equals end) looks one up.
        problems += roadSegmentContext + newGeometry.ValidateRoadSegmentGeometryDomainV2();
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        RebuildSpatialIndexes(logger);

        var (movedRoadNodes, moveProblems) = FindRoadNodesDraggedAlong(roadSegment, newGeometry);
        problems += moveProblems;
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        // Every other segment hanging off one of the moved nodes has to follow it.
        var draggedRoadSegments = GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != roadSegmentId)
            .Where(x => IsConnectedToAnyOf(x, movedRoadNodes.Keys))
            .ToArray();

        // VAL-5: a decentral manager is blocked as soon as one segment in the whole operation is 'ingemeten'.
        foreach (var draggedRoadSegment in draggedRoadSegments)
        {
            problems += ValidateGeometryDrawMethodIsEditable(draggedRoadSegment, mayModifyMeasuredRoadSegments);
        }
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        // The nodes move first: modifying a segment resolves its start and end node by looking up the node sitting on
        // the endpoint, so the nodes have to be where the new geometries expect them before any segment is touched.
        foreach (var (roadNodeId, coordinate) in movedRoadNodes)
        {
            problems += ModifyRoadNode(new ModifyRoadNodeChange
            {
                RoadNodeId = roadNodeId,
                Geometry = RoadNodeGeometry.Create(newGeometry.Value.Factory.CreatePoint(coordinate).WithSrid(newGeometry.SRID))
            }, context);
        }
        if (problems.HasError())
        {
            return Failed(problems, context);
        }

        problems += ModifyRoadSegment(new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
            Geometry = newGeometry,
            AccessRestriction = change.AccessRestriction,
            Category = change.Category,
            Morphology = change.Morphology,
            StreetNameId = change.StreetNameId,
            MaintenanceAuthorityId = change.MaintenanceAuthorityId,
            SurfaceType = change.SurfaceType,
            CarTrafficDirection = change.CarTrafficDirection,
            BikeTrafficDirection = change.BikeTrafficDirection,
            PedestrianTrafficDirection = change.PedestrianTrafficDirection
        }, context);

        foreach (var draggedRoadSegment in draggedRoadSegments)
        {
            problems += DragRoadSegmentAlong(draggedRoadSegment, movedRoadNodes, context);
        }

        if (!problems.HasError())
        {
            problems += VerifyRoadSegmentsAfterChange(context)
                        + VerifyGradeSeparatedJunctionsAfterChange(context);
        }

        // Junctions can appear or disappear when a geometry moves; this recomputes them for everything that changed.
        if (!problems.HasError())
        {
            problems += VerifyAndUpdateJunctions(idGenerator, context);
        }

        if (!problems.HasError() && context.Summary.HasChanges())
        {
            ApplyChangeSummary(context, provenance);
        }

        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    private RoadNetworkChangeResult Failed(Problems problems, ScopedRoadNetworkChangeContext context)
    {
        return new RoadNetworkChangeResult(Problems.None.AddRange(problems.Distinct()), context.Summary);
    }

    // VAL-5
    private static Problems ValidateGeometryDrawMethodIsEditable(RoadSegment roadSegment, bool mayModifyMeasuredRoadSegments)
    {
        if (!mayModifyMeasuredRoadSegments && roadSegment.Attributes?.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten)
        {
            // Reported under the context of the measured segment, which is not necessarily the one in the request:
            // a connected segment dragged along by a moved road node is blocked just the same.
            return Problems.WithContext(roadSegment.RoadSegmentId) + new RoadSegmentChangeGeometryMeasuredNotAllowed();
        }

        return Problems.None;
    }

    // Works out which road nodes are dragged along by the new geometry, and where to. A segment that is not connected
    // to a node (a 'gepland' or 'buiten gebruik' segment is not yet knotted into the network) simply drags nothing.
    private (IReadOnlyDictionary<RoadNodeId, Coordinate> MovedRoadNodes, Problems Problems) FindRoadNodesDraggedAlong(
        RoadSegment roadSegment,
        RoadSegmentGeometry newGeometry)
    {
        if (roadSegment.StartNodeId is null && roadSegment.EndNodeId is null)
        {
            return ([], Problems.None);
        }

        // Whichever of the two endpoints is at fault, it is this segment being moved, so all of it is reported under
        // its context.
        var problems = Problems.WithContext(roadSegment.RoadSegmentId);
        var movedRoadNodes = new Dictionary<RoadNodeId, Coordinate>();

        var currentLine = roadSegment.Geometry.Value.GetSingleLineString();
        var newLine = newGeometry.Value.GetSingleLineString();

        var endpoints = new[]
        {
            (RoadNodeId: roadSegment.StartNodeId, Current: currentLine.Coordinates[0], New: newLine.Coordinates[0]),
            (RoadNodeId: roadSegment.EndNodeId, Current: currentLine.Coordinates[^1], New: newLine.Coordinates[^1])
        };

        // The road nodes this change is about to move - at most the segment's own two endpoints. They are held out of
        // the proximity check below, because the spatial index still has them where they are now rather than where
        // they are going: measuring against a position that is about to be vacated would reject a perfectly good move,
        // and a node moving in from far away is not even returned by a query around the new position. They are
        // compared against each other separately, once both destinations are known.
        var roadNodeIdsBeingMoved = endpoints
            .Where(x => x.RoadNodeId is not null && !x.Current.Equals2D(x.New))
            .Select(x => x.RoadNodeId!.Value)
            .ToHashSet();

        foreach (var endpoint in endpoints)
        {
            if (endpoint.RoadNodeId is null
                || endpoint.Current.Equals2D(endpoint.New)
                || !_roadNodes.TryGetValue(endpoint.RoadNodeId.Value, out var roadNode)
                || roadNode.IsRemoved)
            {
                continue;
            }

            var roadNodeId = endpoint.RoadNodeId.Value;

            // VAL-22
            var maximumDistance = roadNode.Type == RoadNodeTypeV2.Eindknoop
                ? Distances.EndRoadNodeMaximumMoveDistance
                : Distances.RoadNodeMaximumMoveDistance;
            if (endpoint.Current.Distance(endpoint.New) > maximumDistance)
            {
                problems += new RoadSegmentChangeGeometryRoadNodeMovedTooFar(roadNodeId, maximumDistance);
                continue;
            }

            // VAL-21, against the road nodes that stay where they are.
            var minimumDistance = Distances.RoadSegmentChangeGeometryMinimumDistanceToRoadNode;
            var envelope = new Envelope(endpoint.New);
            envelope.ExpandBy(minimumDistance);

            var tooCloseRoadNode = _roadNodesSpatialIndex
                .Query(envelope)
                .Where(x => !x.IsRemoved && !roadNodeIdsBeingMoved.Contains(x.RoadNodeId))
                .FirstOrDefault(x => x.Geometry.Value.Coordinate.Distance(endpoint.New) < minimumDistance);
            if (tooCloseRoadNode is not null)
            {
                problems += new RoadSegmentChangeGeometryPointTooCloseToRoadNode(tooCloseRoadNode.RoadNodeId, minimumDistance);
                continue;
            }

            movedRoadNodes[roadNodeId] = endpoint.New;
        }

        // VAL-21 between the two road nodes this change moves. The spatial index cannot answer this one: it holds
        // both of them at the position they are leaving.
        if (movedRoadNodes.Count == 2)
        {
            var minimumDistance = Distances.RoadSegmentChangeGeometryMinimumDistanceToRoadNode;
            var moved = movedRoadNodes.ToArray();
            if (moved[0].Value.Distance(moved[1].Value) < minimumDistance)
            {
                problems += new RoadSegmentChangeGeometryPointTooCloseToRoadNode(moved[1].Key, minimumDistance);
            }
        }

        return (movedRoadNodes, problems);
    }

    private static bool IsConnectedToAnyOf(RoadSegment roadSegment, IEnumerable<RoadNodeId> roadNodeIds)
    {
        return roadNodeIds.Any(x => roadSegment.StartNodeId == x || roadSegment.EndNodeId == x);
    }

    // Moves the endpoint(s) of a connected segment onto the new road node position(s) and rescales its dynamically
    // segmented attributes over its new length. Nothing else about the segment is touched: the caller never said
    // anything about it, it is only following the node it hangs off.
    private Problems DragRoadSegmentAlong(
        RoadSegment roadSegment,
        IReadOnlyDictionary<RoadNodeId, Coordinate> movedRoadNodes,
        ScopedRoadNetworkChangeContext context)
    {
        var line = roadSegment.Geometry.Value.GetSingleLineString();
        var coordinates = line.Coordinates.ToArray();

        if (roadSegment.StartNodeId is not null && movedRoadNodes.TryGetValue(roadSegment.StartNodeId.Value, out var newStart))
        {
            coordinates[0] = newStart.Copy();
        }
        if (roadSegment.EndNodeId is not null && movedRoadNodes.TryGetValue(roadSegment.EndNodeId.Value, out var newEnd))
        {
            coordinates[^1] = newEnd.Copy();
        }

        var geometry = new MultiLineString([line.Factory.CreateLineString(coordinates)])
            .WithSrid(roadSegment.Geometry.SRID)
            .RoundToCm()
            .ToRoadSegmentGeometry();

        var currentLength = roadSegment.Geometry.Value.Length;
        var newLength = geometry.Value.Length;
        var attributes = roadSegment.Attributes!;

        return ModifyRoadSegment(new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegment.RoadSegmentId),
            Geometry = geometry,
            AccessRestriction = attributes.AccessRestriction.ScaleTo(currentLength, newLength),
            Category = attributes.Category.ScaleTo(currentLength, newLength),
            Morphology = attributes.Morphology.ScaleTo(currentLength, newLength),
            StreetNameId = attributes.StreetNameId.ScaleTo(currentLength, newLength),
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId.ScaleTo(currentLength, newLength),
            SurfaceType = attributes.SurfaceType.ScaleTo(currentLength, newLength),
            CarTrafficDirection = attributes.CarTrafficDirection.ScaleTo(currentLength, newLength),
            BikeTrafficDirection = attributes.BikeTrafficDirection.ScaleTo(currentLength, newLength),
            PedestrianTrafficDirection = attributes.PedestrianTrafficDirection.ScaleTo(currentLength, newLength)
        }, context);
    }
}
