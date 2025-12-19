namespace RoadRegistry.BackOffice.Core;

using System;
using RoadRegistry.RoadSegment.ValueObjects;

public class GradeSeparatedJunction
{
    public GradeSeparatedJunction(
        GradeSeparatedJunctionId id,
        GradeSeparatedJunctionType type,
        RoadSegmentId upperSegment,
        RoadSegmentId lowerSegment)
    {
        Id = id;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        UpperSegment = upperSegment;
        LowerSegment = lowerSegment;
    }

    public GradeSeparatedJunctionId Id { get; }
    public RoadSegmentId LowerSegment { get; }
    public GradeSeparatedJunctionType Type { get; }
    public RoadSegmentId UpperSegment { get; }

    public GradeSeparatedJunction WithLowerSegment(RoadSegmentId value)
    {
        return new GradeSeparatedJunction(Id, Type, UpperSegment, value);
    }

    public GradeSeparatedJunction WithType(GradeSeparatedJunctionType type)
    {
        return new GradeSeparatedJunction(Id, type, UpperSegment, LowerSegment);
    }

    public GradeSeparatedJunction WithUpperSegment(RoadSegmentId value)
    {
        return new GradeSeparatedJunction(Id, Type, value, LowerSegment);
    }
}
