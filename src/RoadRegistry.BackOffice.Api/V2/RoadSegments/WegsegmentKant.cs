namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System;
using RoadRegistry.RoadSegment.ValueObjects;

public enum WegsegmentKant
{
    Links,
    Rechts,
    Beide
}

internal static class WegsegmentKantExtensions
{
    public static WegsegmentKant ToWegsegmentKant(this RoadSegmentAttributeSide side)
    {
        return side switch
        {
            RoadSegmentAttributeSide.Left => WegsegmentKant.Links,
            RoadSegmentAttributeSide.Right => WegsegmentKant.Rechts,
            RoadSegmentAttributeSide.Both => WegsegmentKant.Beide,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }

    public static RoadSegmentAttributeSide ToRoadSegmentAttributeSide(this WegsegmentKant side)
    {
        return side switch
        {
            WegsegmentKant.Links => RoadSegmentAttributeSide.Left,
            WegsegmentKant.Rechts => RoadSegmentAttributeSide.Right,
            WegsegmentKant.Beide => RoadSegmentAttributeSide.Both,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }
}
