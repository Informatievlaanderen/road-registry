namespace RoadRegistry.ScopedRoadNetwork;

using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
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

    public RoadSegmentSplitResult SplitRoadSegment(
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
            return new RoadSegmentSplitResult(problems + new RoadSegmentSplitNotFound(roadSegmentId), []);
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
        if (problems.HasError())
        {
            return new RoadSegmentSplitResult(problems, []);
        }

        var idTranslator = new IdentifierTranslator();
        var context = new ScopedRoadNetworkChangeContext(this, idTranslator, provenance, logger);

        var lineString = segment.Geometry.Value.GetSingleLineString();
        var totalLength = lineString.Length;

        // Project the cut position onto the segment geometry and snap it onto the line.
        var indexedLine = new LengthIndexedLine(lineString);
        var cutMeasure = indexedLine.Project(cutPosition.Coordinate);
        var snappedCoordinate = new Coordinate(indexedLine.ExtractPoint(cutMeasure).X.RoundToCm(), indexedLine.ExtractPoint(cutMeasure).Y.RoundToCm());

        // VAL-8: the cut position must be at least 1m (measured along the segment) from the begin and end node.
        var minimumDistance = Distances.RoadSegmentSplitMinimumDistanceToRoadNode;
        if (cutMeasure < minimumDistance)
        {
            return new RoadSegmentSplitResult(problems + new RoadSegmentSplitPositionTooCloseToRoadNode(segment.StartNodeId ?? RoadNodeId.Zero, minimumDistance), []);
        }
        if (totalLength - cutMeasure < minimumDistance)
        {
            return new RoadSegmentSplitResult(problems + new RoadSegmentSplitPositionTooCloseToRoadNode(segment.EndNodeId ?? RoadNodeId.Zero, minimumDistance), []);
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

        var firstLine = lineString.Factory.CreateLineString(firstCoordinates);
        var secondLine = lineString.Factory.CreateLineString(secondCoordinates);

        var firstGeometry = new MultiLineString([firstLine]).WithSrid(segment.Geometry.SRID).ToRoadSegmentGeometry();
        var secondGeometry = new MultiLineString([secondLine]).WithSrid(segment.Geometry.SRID).ToRoadSegmentGeometry();

        var cutPositionMeasure = new RoadSegmentPositionV2(cutMeasure.RoundToCm());

        // Capture status and attributes before the original is historized (Retire mutates them).
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

        // Historize the original segment before adding the new ones so it no longer participates
        // in the overlap verification of the newly created parts.
        problems += segment.Retire(provenance);
        if (problems.HasError())
        {
            return new RoadSegmentSplitResult(problems, []);
        }
        _roadSegmentsSpatialIndex.Remove(segment.Geometry.Value.EnvelopeInternal, segment);
        context.Summary.RoadSegments.Removed.Add(segment.RoadSegmentId);

        // For a realized segment a new road node (validatieknoop) is inserted at the cut position.
        RoadNodeId? cutNodeId = null;
        if (originalStatus == RoadSegmentStatusV2.Gerealiseerd)
        {
            var temporaryNodeId = _roadNodes.Count > 0 ? _roadNodes.Keys.Max().Next() : new RoadNodeId(1);
            problems += AddRoadNode(new AddRoadNodeChange
            {
                TemporaryId = temporaryNodeId,
                Geometry = RoadNodeGeometry.Create(lineString.Factory.CreatePoint(snappedCoordinate).WithSrid(segment.Geometry.SRID)),
                Grensknoop = false
            }, idGenerator, context);
            if (problems.HasError())
            {
                return new RoadSegmentSplitResult(problems, []);
            }
            cutNodeId = context.Summary.RoadNodes.Added.Last();
        }

        var firstReference = idTranslator.TranslateToTemporaryId(_roadSegments.Keys.Max().Next());
        problems += AddRoadSegment(BuildAddChange(firstReference, firstGeometry, originalStatus, attributes, accessRestriction.First, category.First, morphology.First, streetNameId.First, maintenanceAuthorityId.First, surfaceType.First, carTrafficDirection.First, bikeTrafficDirection.First, pedestrianTrafficDirection.First), idGenerator, context);
        if (problems.HasError())
        {
            return new RoadSegmentSplitResult(problems, []);
        }
        var firstNewRoadSegmentId = context.Summary.RoadSegments.Added.Last();

        var secondReference = idTranslator.TranslateToTemporaryId(_roadSegments.Keys.Max().Next());
        problems += AddRoadSegment(BuildAddChange(secondReference, secondGeometry, originalStatus, attributes, accessRestriction.Second, category.Second, morphology.Second, streetNameId.Second, maintenanceAuthorityId.Second, surfaceType.Second, carTrafficDirection.Second, bikeTrafficDirection.Second, pedestrianTrafficDirection.Second), idGenerator, context);
        if (problems.HasError())
        {
            return new RoadSegmentSplitResult(problems, []);
        }
        var secondNewRoadSegmentId = context.Summary.RoadSegments.Added.Last();

        ReassignGradeSeparatedJunctions(roadSegmentId, firstNewRoadSegmentId, secondNewRoadSegmentId, context);

        // Mark the newly inserted node at the cut position as a validatieknoop so it is preserved
        // (and not merged away) by the topology verification.
        if (cutNodeId is not null)
        {
            _roadNodes[cutNodeId.Value].MarkAsValidatieknoop(provenance);
        }

        problems += AfterChangesApplied(idGenerator, context);
        if (problems.HasError())
        {
            return new RoadSegmentSplitResult(problems, []);
        }

        Apply(new RoadNetworkWasChanged
        {
            RoadNetworkId = RoadNetworkId,
            ScopeGeometry = null,
            DownloadId = null,
            Summary = new RoadNetworkChangedSummary(context.Summary),
            Provenance = new ProvenanceData(provenance)
        });

        return new RoadSegmentSplitResult(Problems.None.AddRange(problems.Distinct()), [firstNewRoadSegmentId, secondNewRoadSegmentId]);
    }

    private static AddRoadSegmentChange BuildAddChange(
        RoadSegmentIdReference reference,
        RoadSegmentGeometry geometry,
        RoadSegmentStatusV2 status,
        RoadSegment.ValueObjects.RoadSegmentAttributes attributes,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> accessRestriction,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> category,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> morphology,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetNameId,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<OrganizationId> maintenanceAuthorityId,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> surfaceType,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> carTrafficDirection,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> bikeTrafficDirection,
        RoadSegment.ValueObjects.RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> pedestrianTrafficDirection)
    {
        return new AddRoadSegmentChange
        {
            RoadSegmentIdReference = reference,
            Geometry = geometry,
            GeometryDrawMethod = attributes.GeometryDrawMethod,
            Status = status,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarTrafficDirection = carTrafficDirection,
            BikeTrafficDirection = bikeTrafficDirection,
            PedestrianTrafficDirection = pedestrianTrafficDirection,
            EuropeanRoadNumbers = attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = attributes.NationalRoadNumbers
        };
    }

    private void ReassignGradeSeparatedJunctions(
        RoadSegmentId originalRoadSegmentId,
        RoadSegmentId firstNewRoadSegmentId,
        RoadSegmentId secondNewRoadSegmentId,
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

            if (junction.LowerRoadSegmentId == originalRoadSegmentId)
            {
                modify = modify with { LowerRoadSegmentId = PickIntersectingPart(junction.UpperRoadSegmentId, firstNewRoadSegmentId, secondNewRoadSegmentId) };
            }

            if (junction.UpperRoadSegmentId == originalRoadSegmentId)
            {
                modify = modify with { UpperRoadSegmentId = PickIntersectingPart(junction.LowerRoadSegmentId, firstNewRoadSegmentId, secondNewRoadSegmentId) };
            }

            junction.Modify(modify, context.Provenance);
            context.Summary.GradeSeparatedJunctions.Modified.Add(junction.GradeSeparatedJunctionId);
        }
    }

    private RoadSegmentId PickIntersectingPart(RoadSegmentId otherRoadSegmentId, RoadSegmentId firstNewRoadSegmentId, RoadSegmentId secondNewRoadSegmentId)
    {
        if (_roadSegments.TryGetValue(otherRoadSegmentId, out var otherSegment))
        {
            var otherGeometry = otherSegment.Geometry.Value;
            if (!_roadSegments[firstNewRoadSegmentId].Geometry.Value.Intersects(otherGeometry)
                && _roadSegments[secondNewRoadSegmentId].Geometry.Value.Intersects(otherGeometry))
            {
                return secondNewRoadSegmentId;
            }
        }

        return firstNewRoadSegmentId;
    }
}

public sealed record RoadSegmentSplitResult(Problems Problems, System.Collections.Generic.IReadOnlyList<RoadSegmentId> RoadSegmentIds);
