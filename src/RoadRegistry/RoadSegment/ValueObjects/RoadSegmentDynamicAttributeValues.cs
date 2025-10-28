namespace RoadRegistry.RoadSegment.ValueObjects;

using System.Collections.Immutable;
using RoadRegistry.BackOffice;

public class RoadSegmentDynamicAttributeValues<T>
{
    //TODO-pr dit object serializen als Values rechstreeks ipv eigen object? if so, via custom JsonConverter
    public ImmutableList<RoadSegmentDynamicAttributeValue<T>> Values { get; private set; } = [];

    public RoadSegmentDynamicAttributeValues<T> Add(T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            From = null,
            To = null,
            Side = RoadSegmentAttributeSide.Both,
            Value = value
        });
        return this;
    }

    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPosition? from, RoadSegmentPosition? to, RoadSegmentAttributeSide side, T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            From = from,
            To = to,
            Side = side,
            Value = value
        });
        return this;
    }
}
