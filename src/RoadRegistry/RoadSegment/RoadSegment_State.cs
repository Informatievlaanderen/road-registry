namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Collections.Immutable;
using Events.V2;
using Extensions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using ValueObjects;

public partial class RoadSegment : MartenAggregateRootEntity<RoadSegmentId>
{
    public RoadSegmentId RoadSegmentId { get; }
    public MultiLineString Geometry { get; private set; }
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
        MultiLineString geometry,
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

    public static RoadSegment Create(RoadSegmentAdded @event)
    {
        var segment = new RoadSegment(@event.RoadSegmentId);
        segment.Apply(@event);
        return segment;
    }
    private void Apply(RoadSegmentAdded @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry.ToGeometry();
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
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public static RoadSegment Create(RoadSegmentMerged @event)
    {
        var segment = new RoadSegment(@event.RoadSegmentId);
        segment.Apply(@event);
        return segment;
    }
    private void Apply(RoadSegmentMerged @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        Geometry = @event.Geometry.ToGeometry();
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
            EuropeanRoadNumbers = @event.EuropeanRoadNumbers.ToImmutableList(),
            NationalRoadNumbers = @event.NationalRoadNumbers.ToImmutableList()
        };
    }

    public void Apply(RoadSegmentModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry?.ToGeometry() ?? Geometry;
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
            SurfaceType = @event.SurfaceType ?? Attributes.SurfaceType
        };
    }

    public void Apply(RoadSegmentRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }

    public void Apply(RoadSegmentRetiredBecauseOfMerger @event)
    {
        UncommittedEvents.Add(@event);

        MergedRoadSegmentId = @event.MergedRoadSegmentId;
        Attributes = Attributes with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.Retired)
        };
    }

    public void Apply(RoadSegmentAddedToEuropeanRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            EuropeanRoadNumbers = Attributes.EuropeanRoadNumbers.Add(@event.Number)
        };
    }
    public void Apply(RoadSegmentRemovedFromEuropeanRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            EuropeanRoadNumbers = Attributes.EuropeanRoadNumbers.Remove(@event.Number)
        };
    }

    public void Apply(RoadSegmentAddedToNationalRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            NationalRoadNumbers = Attributes.NationalRoadNumbers.Add(@event.Number)
        };
    }
    public void Apply(RoadSegmentRemovedFromNationalRoad @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes with
        {
            NationalRoadNumbers = Attributes.NationalRoadNumbers.Remove(@event.Number)
        };
    }
}
