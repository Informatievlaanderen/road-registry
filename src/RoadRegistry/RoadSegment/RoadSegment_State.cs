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
    public RoadNodeId StartNodeId { get; private set; }
    public RoadNodeId EndNodeId { get; private set; }
    public RoadSegmentAttributes Attributes { get; private set; }
    public RoadSegmentId? MergedRoadSegmentId { get; private set; }

    [JsonIgnore]
    public IEnumerable<RoadNodeId> Nodes
    {
        get
        {
            if (StartNodeId > 0)
            {
                yield return StartNodeId;
            }

            if (EndNodeId > 0)
            {
                yield return EndNodeId;
            }
        }
    }

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
        int startNodeId,
        int endNodeId,
        RoadSegmentAttributes attributes,
        bool isRemoved
    )
        : this(new RoadSegmentId(roadSegmentId))
    {
        Geometry = geometry;
        StartNodeId = new RoadNodeId(startNodeId);
        EndNodeId = new RoadNodeId(endNodeId);
        Attributes = attributes;
        IsRemoved = isRemoved;
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
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            Status = @event.Status,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarAccess = @event.CarAccess,
            BikeAccess = @event.BikeAccess,
            PedestrianAccess = @event.PedestrianAccess,
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public static RoadSegment Create(RoadSegmentWasMigrated @event)
    {
        var segment = new RoadSegment(@event.RoadSegmentId);
        segment.Apply(@event);
        return segment;
    }
    private void Apply(RoadSegmentWasMigrated @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry;
        StartNodeId = @event.StartNodeId;
        EndNodeId = @event.EndNodeId;
        Attributes = new RoadSegmentAttributes
        {
            GeometryDrawMethod = @event.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            Status = @event.Status,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarAccess = @event.CarAccess,
            BikeAccess = @event.BikeAccess,
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
            AccessRestriction = @event.AccessRestriction,
            Category = @event.Category,
            Morphology = @event.Morphology,
            Status = @event.Status,
            StreetNameId = @event.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType,
            CarAccess = @event.CarAccess,
            BikeAccess = @event.BikeAccess,
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
        Attributes = Attributes with
        {
            GeometryDrawMethod = @event.GeometryDrawMethod ?? Attributes.GeometryDrawMethod,
            AccessRestriction = @event.AccessRestriction ?? Attributes.AccessRestriction,
            Category = @event.Category ?? Attributes.Category,
            Morphology = @event.Morphology ?? Attributes.Morphology,
            Status = @event.Status ?? Attributes.Status,
            StreetNameId = @event.StreetNameId ?? Attributes.StreetNameId,
            MaintenanceAuthorityId = @event.MaintenanceAuthorityId ?? Attributes.MaintenanceAuthorityId,
            SurfaceType = @event.SurfaceType ?? Attributes.SurfaceType,
            CarAccess = @event.CarAccess ?? Attributes.CarAccess,
            BikeAccess = @event.BikeAccess ?? Attributes.BikeAccess,
            PedestrianAccess = @event.PedestrianAccess ?? Attributes.PedestrianAccess
        };
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

    public void Apply(RoadSegmentWasRetiredBecauseOfMerger @event)
    {
        UncommittedEvents.Add(@event);

        MergedRoadSegmentId = @event.MergedRoadSegmentId;
        Attributes = Attributes with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(RoadSegmentStatusV2.Gehistoreerd, Geometry)
        };
    }

    public void Apply(RoadSegmentWasRetiredBecauseOfMigration @event)
    {
        UncommittedEvents.Add(@event);

        MergedRoadSegmentId = @event.MergedRoadSegmentId;
        Attributes = Attributes with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(RoadSegmentStatusV2.Gehistoreerd, Geometry)
        };
    }

    public void Apply(RoadSegmentWasAddedToEuropeanRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            EuropeanRoadNumbers = Attributes.EuropeanRoadNumbers.Add(@event.Number)
        };
    }
    public void Apply(RoadSegmentWasRemovedFromEuropeanRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            EuropeanRoadNumbers = Attributes.EuropeanRoadNumbers.Remove(@event.Number)
        };
    }

    public void Apply(RoadSegmentWasAddedToNationalRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            NationalRoadNumbers = Attributes.NationalRoadNumbers.Add(@event.Number)
        };
    }
    public void Apply(RoadSegmentWasRemovedFromNationalRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            NationalRoadNumbers = Attributes.NationalRoadNumbers.Remove(@event.Number)
        };
    }
}
