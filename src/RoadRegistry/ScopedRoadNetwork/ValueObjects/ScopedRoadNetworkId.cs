namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using System;

public sealed class ScopedRoadNetworkId
{
    private readonly string _id;

    public ScopedRoadNetworkId(Guid id)
        : this(id.ToString("N"))
    {
    }
    public ScopedRoadNetworkId(string id)
    {
        _id = id;
    }

    public override string ToString() => _id;

    public static implicit operator string(ScopedRoadNetworkId roadNetworkId) => roadNetworkId.ToString();
}
