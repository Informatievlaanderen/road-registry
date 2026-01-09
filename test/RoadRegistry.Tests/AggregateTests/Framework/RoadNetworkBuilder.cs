namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;

public class RoadNetworkBuilder
{
    private readonly IRoadNetworkIdGenerator _idGenerator;
    private readonly List<RoadNetworkChanges> _changes = [];

    public RoadNetworkBuilder(IRoadNetworkIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public ScopedRoadNetwork Build()
    {
        var roadNetwork = new ScopedRoadNetwork(new RoadNetworkId(Guid.NewGuid()));

        foreach (var change in _changes)
        {
            var result = roadNetwork.Change(change, null, _idGenerator);
            if (result.Problems.HasError())
            {
                throw new RoadRegistryProblemsException(result.Problems);
            }
        }

        return roadNetwork.WithoutChanges();
    }

    public RoadNetworkBuilder Add(RoadNetworkChanges roadNetworkChanges)
    {
        _changes.Add(roadNetworkChanges);
        return this;
    }
}
