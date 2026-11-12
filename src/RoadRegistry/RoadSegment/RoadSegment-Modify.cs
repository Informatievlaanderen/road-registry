namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    // A modification can touch the geometry draw method, the status, the geometry and the dynamically segmented
    // attributes all at once, and each of those is recorded by an event of its own, in this order:
    //
    //   1. the geometry draw method - RoadSegmentWasMeasured or RoadSegmentWasCorrectedToOutlined;
    //   2. the status - the status changes of RoadSegmentStatusPaths, one event per hop;
    //   3. the geometry, the road nodes and the attributes - RoadSegmentWasChanged.
    //
    // A path that ends in 'gerealiseerd' knots the segment into the network, and that last hop records the geometry,
    // the road nodes and every attribute in full, so no RoadSegmentWasChanged follows it.
    //
    // Only the status the segment ends up in is validated (a realized segment needs its road nodes), not the statuses
    // a path passes through on its way there.
    public Problems Modify(ModifyRoadSegmentChange change, ScopedRoadNetworkChangeContext context)
    {
        var roadSegmentIdReference = change.RoadSegmentIdReference;
        var problems = Problems.WithContext(roadSegmentIdReference);

        var geometry = (change.Geometry ?? Geometry).Value;

        problems += change.Geometry.ValidateRoadSegmentGeometryDomainV2();

        var segmentLength = geometry.Length;
        var accessRestriction = change.AccessRestriction;
        var category = change.Category;
        var morphology = change.Morphology;
        var streetNameId = change.StreetNameId;
        var maintenanceAuthorityId = change.MaintenanceAuthorityId;
        var surfaceType = change.SurfaceType;
        var carTrafficDirection = change.CarTrafficDirection;
        var bikeTrafficDirection = change.BikeTrafficDirection;
        var pedestrianTrafficDirection = change.PedestrianTrafficDirection;
        var attributes = Attributes! with
        {
            GeometryDrawMethod = change.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = accessRestriction ?? Attributes.AccessRestriction,
            Category = category ?? Attributes.Category,
            Morphology = morphology ?? Attributes.Morphology,
            StreetNameId = streetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = surfaceType ?? Attributes.SurfaceType,
            CarTrafficDirection = carTrafficDirection ?? Attributes.CarTrafficDirection,
            BikeTrafficDirection = bikeTrafficDirection ?? Attributes.BikeTrafficDirection,
            PedestrianTrafficDirection = pedestrianTrafficDirection ?? Attributes.PedestrianTrafficDirection
        };
        problems += new RoadSegmentAttributesValidator().Validate(attributes, segmentLength);

        var status = change.Status ?? Status;
        var statusPath = RoadSegmentStatusPaths.Get(Status, status);

        RoadSegmentNodeIds? nodeIds = null;
        if (status == RoadSegmentStatusV2.Gerealiseerd && (change.Geometry is not null || Status != RoadSegmentStatusV2.Gerealiseerd))
        {
            // A realized segment hangs off a road node at either end, resolved from whatever sits on its endpoints:
            // looked up again when it moves, and for the first time when it becomes realized.
            var startEndNodes = context.RoadNetwork.FindStartEndNodes(change.Geometry ?? Geometry);
            problems += startEndNodes.Problems;
            nodeIds = startEndNodes.NodeIds;
        }

        if (problems.HasError())
        {
            return problems;
        }

        if (nodeIds is not null && nodeIds.Start == StartNodeId && nodeIds.End == EndNodeId)
        {
            nodeIds = null;
        }

        var geometryChanged = change.Geometry is not null && Geometry != change.Geometry;
        var attributesChanged = !Attributes.Equals(attributes with { GeometryDrawMethod = Attributes.GeometryDrawMethod });

        ChangeGeometryDrawMethod(attributes.GeometryDrawMethod, context.Provenance);

        ChangeStatusAlong(statusPath, change.Geometry ?? Geometry, nodeIds, attributes, context.Provenance);

        if (statusPath.Count > 0 && statusPath[^1].Connects)
        {
            return problems;
        }

        if (!geometryChanged && nodeIds is null && !attributesChanged)
        {
            return problems;
        }

        Apply(new RoadSegmentWasChanged
        {
            RoadSegmentId = RoadSegmentId,
            OriginalRoadSegmentIdReference = roadSegmentIdReference,
            Geometry = change.Geometry,
            NodeIds = nodeIds,
            AccessRestriction = accessRestriction,
            Category = category,
            Morphology = morphology,
            StreetNameId = streetNameId,
            MaintenanceAuthorityId = maintenanceAuthorityId,
            SurfaceType = surfaceType,
            CarTrafficDirection = carTrafficDirection,
            BikeTrafficDirection = bikeTrafficDirection,
            PedestrianTrafficDirection = pedestrianTrafficDirection,
            Provenance = new ProvenanceData(context.Provenance)
        });

        return problems;
    }

    // Historizes the segment, along whatever status path leads there from where it is. A segment that is already
    // historized records nothing. Used by an upload that leaves a road segment out: the road it was is kept on record
    // rather than removed.
    public Problems Historize(ScopedRoadNetworkChangeContext context)
    {
        ChangeStatusAlong(
            RoadSegmentStatusPaths.Get(Status, RoadSegmentStatusV2.Gehistoreerd),
            Geometry,
            null,
            Attributes!,
            context.Provenance);

        return Problems.WithContext(RoadSegmentId);
    }

    // Records each status change of a path as its own event. A hop into 'gerealiseerd' records the geometry, the road
    // nodes and the attributes the segment ends up with; nodeIds null there means it keeps the ones it has.
    private void ChangeStatusAlong(
        IReadOnlyList<RoadSegmentStatusChange> path,
        RoadSegmentGeometry geometry,
        RoadSegmentNodeIds? nodeIds,
        RoadSegmentAttributes attributes,
        Provenance provenance)
    {
        foreach (var statusChange in path)
        {
            if (statusChange.Connects)
            {
                ApplyStatusChangeToConnected(statusChange.BuildEvent(new RoadSegmentConnectedChangeData
                {
                    RoadSegmentId = RoadSegmentId,
                    Geometry = geometry,
                    StartNodeId = (nodeIds?.Start ?? StartNodeId)!.Value,
                    EndNodeId = (nodeIds?.End ?? EndNodeId)!.Value,
                    Attributes = attributes,
                    Provenance = new ProvenanceData(provenance)
                }));
            }
            else if (statusChange.Disconnects)
            {
                ApplyStatusChangeFromConnected(statusChange.BuildEvent(new RoadSegmentDisconnectedChangeData
                {
                    RoadSegmentId = RoadSegmentId,
                    PreviousStartNodeId = StartNodeId!.Value,
                    PreviousEndNodeId = EndNodeId!.Value,
                    Provenance = new ProvenanceData(provenance)
                }));
            }
            else
            {
                ApplyStatusChangeWhileUnconnected(statusChange.BuildEvent(new RoadSegmentUnconnectedChangeData
                {
                    RoadSegmentId = RoadSegmentId,
                    Provenance = new ProvenanceData(provenance)
                }));
            }
        }
    }
}
