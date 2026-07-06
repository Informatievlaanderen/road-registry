namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    private static readonly RoadSegmentStatusV2[] SplitAllowedStatuses =
    [
        RoadSegmentStatusV2.Gepland,
        RoadSegmentStatusV2.Gerealiseerd,
        RoadSegmentStatusV2.BuitenGebruik
    ];

    public IReadOnlyList<RoadSegmentId> SplitRoadSegment(
        RoadSegmentId roadSegmentId,
        Point cutPosition,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        var problems = Problems.None;

        if (!_roadSegments.TryGetValue(roadSegmentId, out var segment) || segment.IsRemoved)
        {
            Problems.Single(new RoadSegmentSplitNotFound(roadSegmentId)).ThrowIfError();
            return [];
        }

        // Re-validate the invariants the API also checks: they are not guaranteed here because the
        // domain can be invoked directly and the API request may have become stale in the meantime.
        if (!segment.HasMigrated())
        {
            problems += new RoadSegmentSplitNotCompletedInwinning(roadSegmentId);
        }
        if (!SplitAllowedStatuses.Contains(segment.Status))
        {
            problems += new RoadSegmentSplitStatusNotValid(roadSegmentId);
        }
        if (segment.Geometry.Value.Distance(cutPosition) > Distances.RoadSegmentSplitMaximumDistanceToRoadSegment)
        {
            problems += new RoadSegmentSplitPositionTooFarFromRoadSegment(Distances.RoadSegmentSplitMaximumDistanceToRoadSegment);
        }
        problems.ThrowIfError();

        var idTranslator = new IdentifierTranslator();
        var context = new ScopedRoadNetworkChangeContext(this, idTranslator, provenance, logger);

        var lineString = segment.Geometry.Value.GetSingleLineString();
        var totalLength = lineString.Length;

        // Project the cut position onto the segment geometry and snap it onto the line.
        var indexedLine = new LengthIndexedLine(lineString);
        var cutMeasure = indexedLine.Project(cutPosition.Coordinate).RoundToCm();
        var snappedCoordinate = indexedLine.ExtractPoint(cutMeasure).RoundToCm();

        // VAL-8: the cut position must be at least 1m (measured along the segment) from the begin and end node.
        var minimumDistance = Distances.RoadSegmentSplitMinimumDistanceToRoadNode;
        if (cutMeasure < minimumDistance)
        {
            throw new RoadRegistryProblemsException(Problems.Single(new RoadSegmentSplitPositionTooCloseToRoadNode(segment.StartNodeId ?? RoadNodeId.Zero, minimumDistance)));
        }
        if (totalLength - cutMeasure < minimumDistance)
        {
            throw new RoadRegistryProblemsException(Problems.Single(new RoadSegmentSplitPositionTooCloseToRoadNode(segment.EndNodeId ?? RoadNodeId.Zero, minimumDistance)));
        }

        // Split the geometry at the cut position, preserving the original vertices exactly and
        // forcing the shared endpoint to the snapped cut position.
        var startCoordinate = lineString.Coordinates[0];
        var endCoordinate = lineString.Coordinates[^1];

        var firstCoordinates = ((LineString)indexedLine.ExtractLine(0, cutMeasure)).Coordinates;
        firstCoordinates[0] = startCoordinate;
        firstCoordinates[^1] = snappedCoordinate;

        var secondCoordinates = ((LineString)indexedLine.ExtractLine(cutMeasure, totalLength)).Coordinates;
        secondCoordinates[0] = snappedCoordinate;
        secondCoordinates[^1] = endCoordinate;

        var firstGeometry = new MultiLineString([lineString.Factory.CreateLineString(firstCoordinates)])
            .WithSrid(segment.Geometry.SRID)
            .RoundToCm()
            .ToRoadSegmentGeometry();
        var secondGeometry = new MultiLineString([lineString.Factory.CreateLineString(secondCoordinates)])
            .WithSrid(segment.Geometry.SRID)
            .RoundToCm()
            .ToRoadSegmentGeometry();

        var cutPositionMeasure = new RoadSegmentPositionV2(cutMeasure);

        // Capture status and attributes; split the dynamic attributes into the two parts.
        var originalStatus = segment.Status;
        var attributes = segment.Attributes!;
        var accessRestriction = attributes.AccessRestriction.SplitAt(cutPositionMeasure, totalLength);
        var category = attributes.Category.SplitAt(cutPositionMeasure, totalLength);
        var morphology = attributes.Morphology.SplitAt(cutPositionMeasure, totalLength);
        var streetNameId = attributes.StreetNameId.SplitAt(cutPositionMeasure, totalLength);
        var maintenanceAuthorityId = attributes.MaintenanceAuthorityId.SplitAt(cutPositionMeasure, totalLength);
        var surfaceType = attributes.SurfaceType.SplitAt(cutPositionMeasure, totalLength);
        var carTrafficDirection = attributes.CarTrafficDirection.SplitAt(cutPositionMeasure, totalLength);
        var bikeTrafficDirection = attributes.BikeTrafficDirection.SplitAt(cutPositionMeasure, totalLength);
        var pedestrianTrafficDirection = attributes.PedestrianTrafficDirection.SplitAt(cutPositionMeasure, totalLength);

        var firstPart = new SplitPart(firstGeometry, accessRestriction.First, category.First, morphology.First, streetNameId.First, maintenanceAuthorityId.First, surfaceType.First, carTrafficDirection.First, bikeTrafficDirection.First, pedestrianTrafficDirection.First);
        var secondPart = new SplitPart(secondGeometry, accessRestriction.Second, category.Second, morphology.Second, streetNameId.Second, maintenanceAuthorityId.Second, surfaceType.Second, carTrafficDirection.Second, bikeTrafficDirection.Second, pedestrianTrafficDirection.Second);

        var firstLength = cutMeasure;
        var secondLength = totalLength - cutMeasure;
        var keepIdentifier = System.Math.Max(firstLength, secondLength) / totalLength >= RoadRegistry.RoadSegment.RoadSegmentConstants.MinimumPercentageToKeepIdentifier;

        // Ensure spatial indexes are built once at the start for optimal performance
        RebuildSpatialIndexes(logger);

        // For a realized segment a new road node (validatieknoop) is inserted at the cut position.
        if (originalStatus == RoadSegmentStatusV2.Gerealiseerd)
        {
            problems += AddRoadNode(new AddRoadNodeChange
            {
                TemporaryId = new RoadNodeId(1),
                Geometry = RoadNodeGeometry.Create(lineString.Factory.CreatePoint(snappedCoordinate).WithSrid(segment.Geometry.SRID)),
                Grensknoop = false
            }, idGenerator, context);
            problems.ThrowIfError();
        }

        RoadSegmentId firstPartRoadSegmentId, secondPartRoadSegmentId;
        IReadOnlyList<RoadSegmentId> resultRoadSegmentIds;

        if (keepIdentifier)
        {
            // Situation 2: one part covers >= 70% of the original. The largest part keeps the original
            // identifier (the original segment is modified in place); only the smallest part is added anew.
            var firstIsLongest = firstLength >= secondLength;
            var longestPart = firstIsLongest ? firstPart : secondPart;
            var shortestPart = firstIsLongest ? secondPart : firstPart;

            problems += AddRoadSegment(BuildAddChange(new RoadSegmentIdReference(new RoadSegmentId(1)), originalStatus, attributes, shortestPart), idGenerator, context);
            problems.ThrowIfError();
            var shortestRoadSegmentId = context.Summary.RoadSegments.Added.Last();

            RoadNodeId? longestStartNodeId = null, longestEndNodeId = null;
            if (originalStatus == RoadSegmentStatusV2.Gerealiseerd)
            {
                var startEndNodes = FindStartEndNodes(longestPart.Geometry);
                problems += startEndNodes.Problems;
                problems.ThrowIfError();
                longestStartNodeId = startEndNodes.StartNodeId;
                longestEndNodeId = startEndNodes.EndNodeId;
            }

            var oldEnvelope = segment.Geometry.Value.EnvelopeInternal;
            problems += segment.Split([roadSegmentId, shortestRoadSegmentId], BuildModifications(longestPart, longestStartNodeId, longestEndNodeId), provenance);
            problems.ThrowIfError();
            _roadSegmentsSpatialIndex.Update(oldEnvelope, segment.Geometry.Value.EnvelopeInternal, segment);
            context.Summary.RoadSegments.Modified.Add(roadSegmentId);

            firstPartRoadSegmentId = firstIsLongest ? roadSegmentId : shortestRoadSegmentId;
            secondPartRoadSegmentId = firstIsLongest ? shortestRoadSegmentId : roadSegmentId;
            resultRoadSegmentIds = [roadSegmentId, shortestRoadSegmentId];
        }
        else
        {
            // Situation 1: both parts are smaller than 70% of the original. The original is historized
            // and two new segments are added.
            problems += segment.RetireBecauseOfSplit(provenance);
            problems.ThrowIfError();
            _roadSegmentsSpatialIndex.Remove(segment.Geometry.Value.EnvelopeInternal, segment);
            context.Summary.RoadSegments.Removed.Add(roadSegmentId);

            problems += AddRoadSegment(BuildAddChange(new RoadSegmentIdReference(new RoadSegmentId(1)), originalStatus, attributes, firstPart), idGenerator, context);
            problems.ThrowIfError();
            firstPartRoadSegmentId = context.Summary.RoadSegments.Added.Last();

            problems += AddRoadSegment(BuildAddChange(new RoadSegmentIdReference(new RoadSegmentId(2)), originalStatus, attributes, secondPart), idGenerator, context);
            problems.ThrowIfError();
            secondPartRoadSegmentId = context.Summary.RoadSegments.Added.Last();

            problems += segment.Split([firstPartRoadSegmentId, secondPartRoadSegmentId], null, provenance);
            problems.ThrowIfError();

            resultRoadSegmentIds = [firstPartRoadSegmentId, secondPartRoadSegmentId];
        }

        ReassignGradeSeparatedJunctions(roadSegmentId, firstPartRoadSegmentId, secondPartRoadSegmentId, context);

        // A split performs very specific mutations; only the grade (separated) junctions need to be
        // re-verified. The road node topology verification is intentionally not run here, so the newly
        // inserted validatieknoop is not merged away during this operation.
        problems += VerifyAndUpdateJunctions(idGenerator, context);
        problems.ThrowIfError();

        return resultRoadSegmentIds;
    }

    private static AddRoadSegmentChange BuildAddChange(
        RoadSegmentIdReference reference,
        RoadSegmentStatusV2 status,
        RoadSegment.ValueObjects.RoadSegmentAttributes attributes,
        SplitPart part)
    {
        return new AddRoadSegmentChange
        {
            RoadSegmentIdReference = reference,
            Geometry = part.Geometry,
            GeometryDrawMethod = attributes.GeometryDrawMethod,
            Status = status,
            AccessRestriction = part.AccessRestriction,
            Category = part.Category,
            Morphology = part.Morphology,
            StreetNameId = part.StreetNameId,
            MaintenanceAuthorityId = part.MaintenanceAuthorityId,
            SurfaceType = part.SurfaceType,
            CarTrafficDirection = part.CarTrafficDirection,
            BikeTrafficDirection = part.BikeTrafficDirection,
            PedestrianTrafficDirection = part.PedestrianTrafficDirection,
            EuropeanRoadNumbers = attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = attributes.NationalRoadNumbers
        };
    }

    private static RoadSegmentSplitModifications BuildModifications(SplitPart part, RoadNodeId? startNodeId, RoadNodeId? endNodeId)
    {
        return new RoadSegmentSplitModifications
        {
            Geometry = part.Geometry,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            AccessRestriction = part.AccessRestriction,
            Category = part.Category,
            Morphology = part.Morphology,
            StreetNameId = part.StreetNameId,
            MaintenanceAuthorityId = part.MaintenanceAuthorityId,
            SurfaceType = part.SurfaceType,
            CarTrafficDirection = part.CarTrafficDirection,
            BikeTrafficDirection = part.BikeTrafficDirection,
            PedestrianTrafficDirection = part.PedestrianTrafficDirection
        };
    }

    private void ReassignGradeSeparatedJunctions(
        RoadSegmentId originalRoadSegmentId,
        RoadSegmentId firstPartRoadSegmentId,
        RoadSegmentId secondPartRoadSegmentId,
        ScopedRoadNetworkChangeContext context)
    {
        var connectedJunctions = _gradeSeparatedJunctions
            .Where(x => !x.Value.IsRemoved && x.Value.IsConnectedTo(originalRoadSegmentId))
            .Select(x => x.Value)
            .ToArray();

        foreach (var junction in connectedJunctions)
        {
            var modify = new ModifyGradeSeparatedJunctionChange
            {
                GradeSeparatedJunctionId = junction.GradeSeparatedJunctionId
            };
            var changed = false;

            if (junction.LowerRoadSegmentId == originalRoadSegmentId)
            {
                var newLower = PickIntersectingPart(junction.UpperRoadSegmentId, firstPartRoadSegmentId, secondPartRoadSegmentId);
                if (newLower != junction.LowerRoadSegmentId)
                {
                    modify = modify with { LowerRoadSegmentId = newLower };
                    changed = true;
                }
            }

            if (junction.UpperRoadSegmentId == originalRoadSegmentId)
            {
                var newUpper = PickIntersectingPart(junction.LowerRoadSegmentId, firstPartRoadSegmentId, secondPartRoadSegmentId);
                if (newUpper != junction.UpperRoadSegmentId)
                {
                    modify = modify with { UpperRoadSegmentId = newUpper };
                    changed = true;
                }
            }

            if (!changed)
            {
                continue;
            }

            junction.Modify(modify, context.Provenance);
            context.Summary.GradeSeparatedJunctions.Modified.Add(junction.GradeSeparatedJunctionId);
        }
    }

    private RoadSegmentId PickIntersectingPart(RoadSegmentId otherRoadSegmentId, RoadSegmentId firstPartRoadSegmentId, RoadSegmentId secondPartRoadSegmentId)
    {
        if (_roadSegments.TryGetValue(otherRoadSegmentId, out var otherSegment))
        {
            var otherGeometry = otherSegment.Geometry.Value;
            if (!_roadSegments[firstPartRoadSegmentId].Geometry.Value.Intersects(otherGeometry)
                && _roadSegments[secondPartRoadSegmentId].Geometry.Value.Intersects(otherGeometry))
            {
                return secondPartRoadSegmentId;
            }
        }

        return firstPartRoadSegmentId;
    }

    private sealed record SplitPart(
        RoadSegmentGeometry Geometry,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> CarTrafficDirection,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> BikeTrafficDirection,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> PedestrianTrafficDirection);
}
