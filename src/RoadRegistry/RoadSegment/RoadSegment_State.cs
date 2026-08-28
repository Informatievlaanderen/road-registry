namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;

public partial class RoadSegment : MartenAggregateRootEntity<RoadSegmentId>
{
    public RoadSegmentId RoadSegmentId { get; }
    public RoadSegmentGeometry Geometry { get; private set; }
    public RoadSegmentStatusV2 Status { get; private set; }
    public RoadNodeId? StartNodeId { get; private set; }
    public RoadNodeId? EndNodeId { get; private set; }
    public RoadSegmentAttributes? Attributes { get; private set; }
    public RoadSegmentId? MergedRoadSegmentId { get; private set; }

    // The two road segments this segment was last split into. Populated from RoadSegmentWasSplit so
    // the split result can be recovered from the aggregate state (idempotent handling of a retry).
    public IReadOnlyList<RoadSegmentId>? LastSplitIntoRoadSegmentIds { get; private set; }

    public bool IsRemoved { get; private set; }

    private readonly string? _lastSnapshotEventHash;
    public string LastEventHash => UncommittedEvents.Count > 0 ? UncommittedEvents[^1].GetHash() : _lastSnapshotEventHash ?? string.Empty;

    public bool HasMigrated() => Attributes is not null;

    public RoadSegment(RoadSegmentId id, IEventOrdinalProvider ordinalProvider)
        : base(id, ordinalProvider)
    {
        RoadSegmentId = id;
    }

    [JsonConstructor]
    protected RoadSegment(
        int roadSegmentId,
        RoadSegmentGeometry geometry,
        string status,
        int? startNodeId,
        int? endNodeId,
        RoadSegmentAttributes? attributes,
        bool isRemoved,
        string? lastEventHash,
        IReadOnlyList<RoadSegmentId>? lastSplitIntoRoadSegmentIds
    )
        : this(new RoadSegmentId(roadSegmentId), EventOrdinalProvider.None)
    {
        Geometry = geometry;
        Status = RoadSegmentStatusV2.Parse(status);
        StartNodeId = RoadNodeId.FromValue(startNodeId);
        EndNodeId = RoadNodeId.FromValue(endNodeId);
        Attributes = attributes;
        IsRemoved = isRemoved;
        _lastSnapshotEventHash = lastEventHash;
        LastSplitIntoRoadSegmentIds = lastSplitIntoRoadSegmentIds;
    }

    public IEnumerable<RoadNodeId> GetNodeIds()
    {
        if (StartNodeId > 0)
        {
            yield return StartNodeId.Value;
        }

        if (EndNodeId > 0)
        {
            yield return EndNodeId.Value;
        }
    }

    public static RoadSegment CreateForMigration(
        RoadSegmentId roadSegmentId,
        RoadSegmentGeometry geometry,
        RoadSegmentStatusV2 status,
        RoadNodeId? startNodeId,
        RoadNodeId? endNodeId)
    {
        return new RoadSegment(roadSegmentId, geometry, status.ToString(), startNodeId, endNodeId, null, false, null, null);
    }

    public static RoadSegment Create(RoadSegmentWasAdded @event)
    {
        return CreateWithProvider(@event, EventOrdinalProvider.None);
    }

    // Deliberately not named "Create": Marten's snapshot aggregation discovers static Create methods and can only
    // resolve parameters it knows about, so it would reject the IEventOrdinalProvider. This overload is used by the
    // domain Add path to stamp the created event with the change's ordinal provider.
    private static RoadSegment CreateWithProvider(RoadSegmentWasAdded @event, IEventOrdinalProvider ordinalProvider)
    {
        var segment = new RoadSegment(@event.RoadSegmentId, ordinalProvider);
        segment.Apply(@event);
        return segment;
    }
    private void Apply(RoadSegmentWasAdded @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        Status = @event.Status;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarTrafficDirection = @event.CarTrafficDirection,
            BikeTrafficDirection = @event.BikeTrafficDirection,
            PedestrianTrafficDirection = @event.PedestrianTrafficDirection,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public static RoadSegment Create(OutlinedRoadSegmentWasAdded @event)
    {
        var segment = new RoadSegment(@event.RoadSegmentId, EventOrdinalProvider.None);
        segment.Apply(@event);
        return segment;
    }
    private void Apply(OutlinedRoadSegmentWasAdded @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        Status = @event.Status;
        StartNodeId = null;
        EndNodeId = null;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarTrafficDirection = @event.CarTrafficDirection,
            BikeTrafficDirection = @event.BikeTrafficDirection,
            PedestrianTrafficDirection = @event.PedestrianTrafficDirection,
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
    }

    public void Apply(RoadSegmentWasMigrated @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        Status = @event.Status;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarTrafficDirection = @event.CarTrafficDirection,
            BikeTrafficDirection = @event.BikeTrafficDirection,
            PedestrianTrafficDirection = @event.PedestrianTrafficDirection,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public void Apply(RoadSegmentWasMerged @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        Status = @event.Status;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarTrafficDirection = @event.CarTrafficDirection,
            BikeTrafficDirection = @event.BikeTrafficDirection,
            PedestrianTrafficDirection = @event.PedestrianTrafficDirection,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public void Apply(RoadSegmentWasModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry ?? Geometry;
        Status = @event.Status ?? Status;
        if (@event.NodeIds is not null)
        {
            StartNodeId = @event.NodeIds.Start;
            EndNodeId = @event.NodeIds.End;
        }
        Attributes = Attributes! with
        {
            GeometryDrawMethod = @event.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction ?? Attributes.AccessRestriction,
            Category = @event.Category ?? Attributes.Category,
            Morphology = @event.Morphology ?? Attributes.Morphology,
            StreetNameId = @event.StreetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType ?? Attributes.SurfaceType,
            CarTrafficDirection = @event.CarTrafficDirection ?? Attributes.CarTrafficDirection,
            BikeTrafficDirection = @event.BikeTrafficDirection ?? Attributes.BikeTrafficDirection,
            PedestrianTrafficDirection = @event.PedestrianTrafficDirection ?? Attributes.PedestrianTrafficDirection
        };
    }

    // The status change events. Every one of them is applied by the shared handler for its kind - the transition it
    // stands for is looked up from the event type - but each concrete event keeps an Apply of its own because that is
    // what Marten's snapshot aggregation binds to.

    public void Apply(RoadSegmentWasRealizedFromPlanned @event) => ApplyStatusChangeToConnected(@event);
    public void Apply(RoadSegmentWasRealizedFromOutOfUse @event) => ApplyStatusChangeToConnected(@event);
    public void Apply(RoadSegmentWasCorrectedFromHistorizedToRealized @event) => ApplyStatusChangeToConnected(@event);

    public void Apply(RoadSegmentWasCorrectedFromRealizedToPlanned @event) => ApplyStatusChangeFromConnected(@event);
    public void Apply(RoadSegmentWasTakenOutOfUseFromRealized @event) => ApplyStatusChangeFromConnected(@event);
    public void Apply(RoadSegmentWasHistorizedFromRealized @event) => ApplyStatusChangeFromConnected(@event);

    public void Apply(RoadSegmentWasHistorizedFromOutOfUse @event) => ApplyStatusChangeWhileUnconnected(@event);
    public void Apply(RoadSegmentWasCorrectedFromNotRealizedToPlanned @event) => ApplyStatusChangeWhileUnconnected(@event);
    public void Apply(RoadSegmentWasCorrectedFromHistorizedToOutOfUse @event) => ApplyStatusChangeWhileUnconnected(@event);

    // Knotted into the network: the event records the realized state in full, so nothing is left at what it was.
    private void ApplyStatusChangeToConnected(IRoadSegmentWasConnectedEvent @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry;
        Status = RoadSegmentStatusChange.ForEvent(@event).To;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = Attributes! with
        {
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarTrafficDirection = @event.CarTrafficDirection,
            BikeTrafficDirection = @event.BikeTrafficDirection,
            PedestrianTrafficDirection = @event.PedestrianTrafficDirection
        };
    }

    // Come loose from the network: the geometry and every attribute stay as they were, only the status changes and
    // the road nodes are given up.
    private void ApplyStatusChangeFromConnected(IRoadSegmentWasDisconnectedEvent @event)
    {
        UncommittedEvents.Add(@event);

        Status = RoadSegmentStatusChange.ForEvent(@event).To;

        // Only a realized segment is knotted into the network; a segment in any other status carries no road nodes.
        StartNodeId = null;
        EndNodeId = null;
    }

    // Outside the network before and after, so nothing but the status moves.
    private void ApplyStatusChangeWhileUnconnected(IRoadSegmentUnconnectedStatusChangeEvent @event)
    {
        UncommittedEvents.Add(@event);

        Status = RoadSegmentStatusChange.ForEvent(@event).To;
    }

    public void Apply(RoadSegmentGeometryWasModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
    }

    public void Apply(RoadSegmentWasRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }

    public void Apply(RoadSegmentWasRemovedBecauseOfMigration @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }

    public void Apply(RoadSegmentWasRetired @event)
    {
        UncommittedEvents.Add(@event);

        Status = RoadSegmentStatusV2.Gehistoreerd;
        StartNodeId = null;
        EndNodeId = null;
    }

    public void Apply(RoadSegmentWasRetiredBecauseOfMerger @event)
    {
        UncommittedEvents.Add(@event);

        MergedRoadSegmentId = @event.MergedRoadSegmentId;
        Status = RoadSegmentStatusV2.Gehistoreerd;
        StartNodeId = null;
        EndNodeId = null;
    }

    public void Apply(RoadSegmentWasRetiredBecauseOfSplit @event)
    {
        UncommittedEvents.Add(@event);

        Status = RoadSegmentStatusV2.Gehistoreerd;
        StartNodeId = null;
        EndNodeId = null;
    }

    public void Apply(RoadSegmentWasSplit @event)
    {
        UncommittedEvents.Add(@event);

        LastSplitIntoRoadSegmentIds = @event.NewRoadSegmentIds;

        if (@event.Modifications is not null)
        {
            Geometry = @event.Modifications.Geometry;
            StartNodeId = @event.Modifications.StartNodeId;
            EndNodeId = @event.Modifications.EndNodeId;
            Attributes = Attributes! with
            {
                AccessRestriction = @event.Modifications.AccessRestriction,
                Category = @event.Modifications.Category,
                Morphology = @event.Modifications.Morphology,
                StreetNameId = @event.Modifications.StreetNameId,
                MaintenanceAuthorityId = @event.Modifications.MaintenanceAuthorityId,
                SurfaceType = @event.Modifications.SurfaceType,
                CarTrafficDirection = @event.Modifications.CarTrafficDirection,
                BikeTrafficDirection = @event.Modifications.BikeTrafficDirection,
                PedestrianTrafficDirection = @event.Modifications.PedestrianTrafficDirection
            };
        }
    }

    public void Apply(RoadSegmentWasAddedToEuropeanRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            EuropeanRoadNumbers = Attributes.EuropeanRoadNumbers.Add(@event.Number)
        };
    }
    public void Apply(RoadSegmentWasRemovedFromEuropeanRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            EuropeanRoadNumbers = Attributes.EuropeanRoadNumbers.Remove(@event.Number)
        };
    }

    public void Apply(RoadSegmentWasAddedToNationalRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            NationalRoadNumbers = Attributes.NationalRoadNumbers.Add(@event.Number)
        };
    }
    public void Apply(RoadSegmentWasRemovedFromNationalRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            NationalRoadNumbers = Attributes.NationalRoadNumbers.Remove(@event.Number)
        };
    }
}
