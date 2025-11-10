namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Collections.Immutable;
using BackOffice;
using Events;
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

    [JsonIgnore]
    public bool IsRemoved { get; private set; }

    public RoadSegment(RoadSegmentId id)
        : base(id)
    {
        RoadSegmentId = id;
    }

    [JsonConstructor]
    public RoadSegment(
        int roadSegmentId,
        MultiLineString geometry,
        int startNodeId,
        int endNodeId,
        RoadSegmentAttributes attributes
    )
        : this(new RoadSegmentId(roadSegmentId))
    {
        Geometry = geometry;
        StartNodeId = new RoadNodeId(startNodeId);
        EndNodeId = new RoadNodeId(endNodeId);
        Attributes = attributes;
    }

    public static RoadSegment Create(RoadSegmentAdded @event)
    {
        var segment = new RoadSegment(@event.RoadSegmentId)
        {
            Geometry = @event.Geometry.AsMultiLineString(),
            StartNodeId = @event.StartNodeId,
            EndNodeId = @event.EndNodeId,
            Attributes = new()
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
            }
        };
        segment.UncommittedEvents.Add(@event);
        return segment;
    }

    public void Apply(RoadSegmentModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry.AsMultiLineString();
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

    public void Apply(RoadSegmentRemoved @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = true;
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
