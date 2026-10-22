namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;

public partial class ScopedRoadNetwork
{
    // Inwinning runs per municipality, and two neighbours are collected independently of one another. Whichever upload
    // lands first may merge a road segment that runs across the border with the one next to it, and that merge removes
    // the road node the two met at. The second upload was prepared against the extract from before all that: it still
    // names the road node that is now gone, and it still carries the stretch of road the merge already took.
    //
    // Refusing it outright would leave the second municipality stuck behind its neighbour. Instead, the stretch that
    // was already collected is cut off what the second upload brings, and the rest is collected as usual. The cut is
    // made in the second upload rather than in what is already on record, so the attributes of the upload that landed
    // first are the ones that survive over the stretch the two share.
    //
    // This is particular to inwinning: it is the only upload that migrates road segments it did not draw itself.

    // How far along the uploaded road segment, away from the road node, to look for the segment that took its place.
    // Far enough off the road node that only the roads actually leaving it that way are in reach, and the register
    // works to the centimetre, so nothing finer could be told apart anyway.
    private const double NeighbourMergeProbeDistance = 0.01;

    // What is left of an uploaded road segment once the stretch an earlier inwinning already took is cut off.
    // TakenFromStart says at which end of the uploaded geometry that stretch sat, which is what the dynamically
    // segmented attributes have to be cut back by: they are positioned along the geometry as it was uploaded.
    private sealed record NeighbourMergeResolution(RoadSegmentGeometry? RemainingGeometry, bool TakenFromStart)
    {
        // The earlier merge took the whole of it: there is nothing left for this upload to collect.
        public bool TakenInFull => RemainingGeometry is null;
    }

    private sealed record NeighbourMergeResolutions(
        IReadOnlyCollection<RoadNodeId> RoadNodeIds,
        IReadOnlyDictionary<RoadSegmentIdReference, NeighbourMergeResolution> RoadSegments)
    {
        public static readonly NeighbourMergeResolutions None =
            new([], new Dictionary<RoadSegmentIdReference, NeighbourMergeResolution>());
    }

    // Works out, before a single change is applied, which of them are about road segments a neighbouring inwinning
    // already merged away. A road node this upload wants to migrate but that the road network no longer has is the
    // giveaway: an inwinning that merged the roads meeting there is the only thing that can have removed it.
    private NeighbourMergeResolutions ResolveNeighbourMerges(RoadNetworkChanges changes, ScopedRoadNetworkChangeContext context)
    {
        var removedRoadNodes = changes
            .OfType<ModifyRoadNodeChange>()
            .Where(change => change.Geometry is not null && !_roadNodes.ContainsKey(change.RoadNodeId))
            .ToArray();

        if (removedRoadNodes.Length == 0)
        {
            return NeighbourMergeResolutions.None;
        }

        var resolvedRoadNodeIds = new List<RoadNodeId>();
        var resolvedRoadSegments = new Dictionary<RoadSegmentIdReference, NeighbourMergeResolution>();

        var roadSegmentChanges = changes
            .OfType<ModifyRoadSegmentChange>()
            .Where(change => change.Geometry is not null)
            .ToArray();

        foreach (var removedRoadNode in removedRoadNodes)
        {
            var roadNodeCoordinate = removedRoadNode.Geometry!.Value.Coordinate;

            var accountedFor = false;

            // Only the road segments this upload runs up to that road node can have been merged away along with it.
            foreach (var roadSegmentChange in roadSegmentChanges.Where(x => TouchesCoordinate(x.Geometry!.Value, roadNodeCoordinate)))
            {
                if (resolvedRoadSegments.ContainsKey(roadSegmentChange.RoadSegmentIdReference))
                {
                    continue;
                }

                var uploadedGeometry = roadSegmentChange.Geometry!.Value.GetSingleLineString();

                var takenOverBy = FindRoadSegmentLeavingRoadNode(
                    uploadedGeometry, roadNodeCoordinate, roadSegmentChange.RoadSegmentIdReference.RoadSegmentId);
                if (takenOverBy is null)
                {
                    continue;
                }

                var resolution = CutOffStretchAlreadyTaken(uploadedGeometry, roadNodeCoordinate, takenOverBy);
                if (resolution is null)
                {
                    continue;
                }

                resolvedRoadSegments.Add(roadSegmentChange.RoadSegmentIdReference, resolution);
                accountedFor = true;

                context.Logger.LogInformation(
                    "Road node {RoadNodeId} was merged away by another inwinning: road segment {RoadSegmentId} was taken over by road segment {TakenOverBy} and keeps {Remaining}",
                    removedRoadNode.RoadNodeId.ToInt32(),
                    roadSegmentChange.RoadSegmentIdReference.RoadSegmentId.ToInt32(),
                    takenOverBy.RoadSegmentId.ToInt32(),
                    resolution.TakenInFull ? "nothing of itself" : "what is left of itself");
            }

            // The road node is only let go once it is clear what became of the roads that ran up to it. Short of that
            // it is simply a road node this upload cannot account for, which is the caller's to report as before.
            if (accountedFor)
            {
                resolvedRoadNodeIds.Add(removedRoadNode.RoadNodeId);
            }
        }

        return new NeighbourMergeResolutions(resolvedRoadNodeIds, resolvedRoadSegments);
    }

    private static bool TouchesCoordinate(MultiLineString geometry, Coordinate coordinate)
    {
        var line = geometry.GetSingleLineString();

        return line.StartPoint.Coordinate.Distance(coordinate) <= NeighbourMergeProbeDistance
               || line.EndPoint.Coordinate.Distance(coordinate) <= NeighbourMergeProbeDistance;
    }

    // The road segment that now lies where the uploaded one leaves the road node. Probed a centimetre along the
    // uploaded geometry rather than at the road node itself, where every road meeting there would answer.
    private RoadSegment? FindRoadSegmentLeavingRoadNode(LineString uploadedGeometry, Coordinate roadNodeCoordinate, RoadSegmentId uploadedRoadSegmentId)
    {
        var awayFromRoadNode = OrientAwayFrom(uploadedGeometry, roadNodeCoordinate);
        var probe = new Point(new LengthIndexedLine(awayFromRoadNode)
            .ExtractPoint(Math.Min(NeighbourMergeProbeDistance, awayFromRoadNode.Length)));

        var envelope = new Envelope(probe.Coordinate);
        envelope.ExpandBy(NeighbourMergeProbeDistance);

        return _roadSegmentsSpatialIndex
            .Query(envelope)
            .Where(x => !x.IsRemoved && x.RoadSegmentId != uploadedRoadSegmentId)
            .Where(x => x.Geometry.Value.Distance(probe) <= NeighbourMergeProbeDistance)
            .OrderBy(x => x.Geometry.Value.Distance(probe))
            .FirstOrDefault();
    }

    // Cuts the stretch the other inwinning already took off the uploaded geometry and answers what is left, or that it
    // was taken in full. Null when the two do not line up well enough to tell: then the upload is left exactly as it
    // came, and the road node it cannot account for is reported as it was before.
    private static NeighbourMergeResolution? CutOffStretchAlreadyTaken(LineString uploadedGeometry, Coordinate roadNodeCoordinate, RoadSegment takenOverBy)
    {
        var awayFromRoadNode = OrientAwayFrom(uploadedGeometry, roadNodeCoordinate);
        var indexedUpload = new LengthIndexedLine(awayFromRoadNode);

        // The road segment that took over ends somewhere on the uploaded geometry - at the road node both uploads
        // still share. Everything between the removed road node and that point has already been collected.
        var takenOverLine = takenOverBy.Geometry.Value.GetSingleLineString();

        double? cutIndex = null;
        foreach (var endPoint in new[] { takenOverLine.StartPoint.Coordinate, takenOverLine.EndPoint.Coordinate })
        {
            var index = indexedUpload.Project(endPoint);
            if (indexedUpload.ExtractPoint(index).Distance(endPoint) > NeighbourMergeProbeDistance)
            {
                continue;
            }

            if (cutIndex is null || index > cutIndex)
            {
                cutIndex = index;
            }
        }

        if (cutIndex is null)
        {
            return null;
        }

        // The road node sits at the start of the uploaded geometry whenever reading it from that end did not turn it
        // around - which is exactly what OrientAwayFrom leaves behind.
        var takenFromStart = ReferenceEquals(awayFromRoadNode, uploadedGeometry);

        if (awayFromRoadNode.Length - cutIndex.Value <= NeighbourMergeProbeDistance)
        {
            return new NeighbourMergeResolution(null, takenFromStart);
        }

        var remaining = (LineString)indexedUpload.ExtractLine(cutIndex.Value, awayFromRoadNode.Length);

        // Handed back the way it was uploaded, so that what follows does not have to care which end was cut.
        if (!takenFromStart)
        {
            remaining = (LineString)remaining.Reverse();
        }

        return new NeighbourMergeResolution(new MultiLineString([remaining])
        {
            SRID = uploadedGeometry.SRID
        }.ToRoadSegmentGeometry(), takenFromStart);
    }

    // The uploaded change, cut back to the stretch this upload still has to collect. The dynamically segmented
    // attributes are cut along with the geometry: they are positioned along the road, so a road that lost a stretch
    // at one end has attribute positions that no longer line up with it.
    private static ModifyRoadSegmentChange CutBackToRemainingStretch(ModifyRoadSegmentChange change, NeighbourMergeResolution resolution)
    {
        var totalLength = change.Geometry!.Value.Length;
        var remainingLength = resolution.RemainingGeometry!.Value.Length;
        var takenLength = totalLength - remainingLength;

        var cutPosition = new RoadSegmentPositionV2(resolution.TakenFromStart ? takenLength : remainingLength);
        var firstLength = resolution.TakenFromStart ? takenLength : remainingLength;
        var secondLength = resolution.TakenFromStart ? remainingLength : takenLength;

        RoadSegmentDynamicAttributeValues<T>? CutBack<T>(RoadSegmentDynamicAttributeValues<T>? attributes)
        {
            if (attributes is null)
            {
                return null;
            }

            var (first, second) = attributes.SplitAt(cutPosition, totalLength, firstLength, secondLength);

            return resolution.TakenFromStart ? second : first;
        }

        return change with
        {
            Geometry = resolution.RemainingGeometry,
            AccessRestriction = CutBack(change.AccessRestriction),
            Category = CutBack(change.Category),
            Morphology = CutBack(change.Morphology),
            StreetNameId = CutBack(change.StreetNameId),
            MaintenanceAuthorityId = CutBack(change.MaintenanceAuthorityId),
            SurfaceType = CutBack(change.SurfaceType),
            CarTrafficDirection = CutBack(change.CarTrafficDirection),
            BikeTrafficDirection = CutBack(change.BikeTrafficDirection),
            PedestrianTrafficDirection = CutBack(change.PedestrianTrafficDirection)
        };
    }

    // The geometry read from the end that sits on the road node, so that an index of 0 is the road node itself.
    private static LineString OrientAwayFrom(LineString geometry, Coordinate coordinate)
    {
        return geometry.StartPoint.Coordinate.Distance(coordinate) <= geometry.EndPoint.Coordinate.Distance(coordinate)
            ? geometry
            : (LineString)geometry.Reverse();
    }
}
