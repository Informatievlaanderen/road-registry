namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Collections.Immutable;
using Events.V2;
using Newtonsoft.Json;
using ValueObjects;

public partial class RoadSegment : MartenAggregateRootEntity<RoadSegmentId>
{
    public RoadSegmentId RoadSegmentId { get; }
    public RoadSegmentGeometry Geometry { get; private set; }
    public RoadNodeId? StartNodeId { get; private set; }
    public RoadNodeId? EndNodeId { get; private set; }
    public RoadSegmentAttributes? Attributes { get; private set; }
    public RoadSegmentId? MergedRoadSegmentId { get; private set; }

    public bool IsRemoved { get; private set; }

    public RoadSegment(RoadSegmentId id)
        : base(id)
    {
        RoadSegmentId = id;
    }

    [JsonConstructor]
    protected RoadSegment(
        int roadSegmentId,
        RoadSegmentGeometry geometry,
        int? startNodeId,
        int? endNodeId,
        RoadSegmentAttributes? attributes,
        bool isRemoved
    )
        : this(new RoadSegmentId(roadSegmentId))
    {
        Geometry = geometry;
        StartNodeId = RoadNodeId.FromValue(startNodeId);
        EndNodeId = RoadNodeId.FromValue(endNodeId);
        Attributes = attributes;
        IsRemoved = isRemoved;
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
        RoadNodeId? startNodeId,
        RoadNodeId? endNodeId)
    {
        return new RoadSegment(roadSegmentId, geometry, startNodeId, endNodeId, null, false);
    }

    public static RoadSegment Create(RoadSegmentWasAdded @event)
    {
        var segment = new RoadSegment(@event.RoadSegmentId);
        segment.Apply(@event);
        return segment;
    }
    private void Apply(RoadSegmentWasAdded @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            Status = @event.Status,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarAccessForward = @event.CarAccessForward,
            CarAccessBackward = @event.CarAccessBackward,
            BikeAccessForward = @event.BikeAccessForward,
            BikeAccessBackward = @event.BikeAccessBackward,
            PedestrianAccess = @event.PedestrianAccess,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public void Apply(RoadSegmentWasMigrated @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            Status = @event.Status,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarAccessForward = @event.CarAccessForward,
            CarAccessBackward = @event.CarAccessBackward,
            BikeAccessForward = @event.BikeAccessForward,
            BikeAccessBackward = @event.BikeAccessBackward,
            PedestrianAccess = @event.PedestrianAccess,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public static RoadSegment Create(RoadSegmentWasMerged @event)
    {
        var segment = new RoadSegment(@event.RoadSegmentId);
        segment.Apply(@event);
        return segment;
    }
    private void Apply(RoadSegmentWasMerged @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            Status = @event.Status,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarAccessForward = @event.CarAccessForward,
            CarAccessBackward = @event.CarAccessBackward,
            BikeAccessForward = @event.BikeAccessForward,
            BikeAccessBackward = @event.BikeAccessBackward,
            PedestrianAccess = @event.PedestrianAccess,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public void Apply(RoadSegmentWasModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry ?? Geometry;
        StartNodeId = @event.StartNodeId ?? StartNodeId;
        EndNodeId = @event.EndNodeId ?? EndNodeId;
        Attributes = Attributes! with
        {
            GeometryDrawMethod = @event.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            Status = @event.Status ?? Attributes.Status,
            AccessRestriction = @event.AccessRestriction ?? Attributes.AccessRestriction,
            Category = @event.Category ?? Attributes.Category,
            Morphology = @event.Morphology ?? Attributes.Morphology,
            StreetNameId = @event.StreetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType ?? Attributes.SurfaceType,
            CarAccessForward = @event.CarAccessForward ?? Attributes.CarAccessForward,
            CarAccessBackward = @event.CarAccessBackward ?? Attributes.CarAccessBackward,
            BikeAccessForward = @event.BikeAccessForward ?? Attributes.BikeAccessForward,
            BikeAccessBackward = @event.BikeAccessBackward ?? Attributes.BikeAccessBackward,
            PedestrianAccess = @event.PedestrianAccess ?? Attributes.PedestrianAccess
        };
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

        Attributes = Attributes! with
        {
            Status = RoadSegmentStatusV2.Gehistoreerd
        };
    }

    public void Apply(RoadSegmentWasRetiredBecauseOfMerger @event)
    {
        UncommittedEvents.Add(@event);

        MergedRoadSegmentId = @event.MergedRoadSegmentId;
        Attributes = Attributes! with
        {
            Status = RoadSegmentStatusV2.Gehistoreerd
        };
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
