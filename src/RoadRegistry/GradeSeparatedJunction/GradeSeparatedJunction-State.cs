namespace RoadRegistry.GradeSeparatedJunction;

using System;
using BackOffice;
using RoadSegment.ValueObjects;

public partial class GradeSeparatedJunction
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId LowerSegment { get; private set; }
    public RoadSegmentId UpperSegment { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }

    public GradeSeparatedJunction(GradeSeparatedJunctionId id)
        : base(id)
    {
        GradeSeparatedJunctionId = id;
    }

    public static GradeSeparatedJunction Create(object @event) //GradeSeparatedJunctionAdded
    {
        return new GradeSeparatedJunction(new GradeSeparatedJunctionId(0)) //@event.Id
        {
            //RoadNodeId = @event.Id,

            //LastEventHash = @event.GetHash();
        };
    }
}
