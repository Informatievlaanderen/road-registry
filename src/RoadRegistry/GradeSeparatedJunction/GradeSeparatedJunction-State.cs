﻿namespace RoadRegistry.GradeSeparatedJunction;

using System.Text.Json.Serialization;
using BackOffice;
using Events;
using RoadSegment.ValueObjects;

public partial class GradeSeparatedJunction
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; }
    public RoadSegmentId LowerRoadSegmentId { get; private set; }
    public RoadSegmentId UpperRoadSegmentId { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }

    public GradeSeparatedJunction(GradeSeparatedJunctionId id)
        : base(id)
    {
        GradeSeparatedJunctionId = id;
    }

    [JsonConstructor]
    public GradeSeparatedJunction(
        int gradeSeparatedJunctionId,
        int lowerRoadSegmentId,
        int upperRoadSegmentId,
        string type
    )
        : this(new GradeSeparatedJunctionId(gradeSeparatedJunctionId))
    {
        LowerRoadSegmentId = new RoadSegmentId(lowerRoadSegmentId);
        UpperRoadSegmentId = new RoadSegmentId(upperRoadSegmentId);
        Type = GradeSeparatedJunctionType.Parse(type);
    }

    public static GradeSeparatedJunction Create(GradeSeparatedJunctionAdded @event)
    {
        var junction = new GradeSeparatedJunction(@event.Id)
        {
            LowerRoadSegmentId = @event.LowerRoadSegmentId,
            UpperRoadSegmentId = @event.UpperRoadSegmentId,
            Type = @event.Type
            //LastEventHash = @event.GetHash();
        };
        junction.UncommittedEvents.Add(@event);
        return junction;
    }
}
