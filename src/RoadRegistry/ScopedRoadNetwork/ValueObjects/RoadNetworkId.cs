namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using System;

public sealed class RoadNetworkId
{
    private readonly string _id;

    public RoadNetworkId(Guid id)
        : this(id.ToString("N"))
    {
    }
    public RoadNetworkId(string id)
    {
        _id = id;
    }

    public override string ToString() => _id;

    public static implicit operator string(RoadNetworkId roadNetworkId) => roadNetworkId.ToString();
}
