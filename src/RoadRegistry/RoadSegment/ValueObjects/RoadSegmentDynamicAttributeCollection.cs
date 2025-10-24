namespace RoadRegistry.RoadSegment.ValueObjects;

using System.Collections.Generic;
using RoadRegistry.BackOffice;

public class RoadSegmentDynamicAttributeCollection<T>: List<RoadSegmentDynamicAttributeValue<T>>
{
    public void Set(T value)
    {
        Clear();
        Add(null, null, RoadSegmentAttributeSide.Both, value);
    }

    public void Add(RoadSegmentPosition? from, RoadSegmentPosition? to, RoadSegmentAttributeSide side, T value)
    {
        Add(new RoadSegmentDynamicAttributeValue<T>
        {
            From = from,
            To = to,
            Side = side,
            Value = value
        });
    }
}
