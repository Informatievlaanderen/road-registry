namespace RoadRegistry.RoadNetwork.ValueObjects;

using System;

public sealed class RoadNetworkId
{
    private readonly string _id;

    public RoadNetworkId(Guid id)
    {
        _id = id.ToString("N");
    }
    private RoadNetworkId(string streamId)
    {
        _id = streamId;
    }

    public static RoadNetworkId FromStreamId(string streamId)
    {
        return new RoadNetworkId(streamId);
    }

    public override string ToString() => _id;

    public static implicit operator string(RoadNetworkId roadNetworkId) => roadNetworkId.ToString();
}
