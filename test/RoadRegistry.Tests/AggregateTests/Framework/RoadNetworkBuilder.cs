namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.RoadNetwork;
using RoadNetwork = RoadRegistry.RoadNetwork.RoadNetwork;

public class RoadNetworkBuilder
{
    private readonly IRoadNetworkIdGenerator _idGenerator;
    private readonly List<RoadNetworkChanges> _changes = [];

    public RoadNetworkBuilder(IRoadNetworkIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public RoadNetwork Build()
    {
        var roadNetwork = RoadNetwork.Empty;

        foreach (var change in _changes)
        {
            var result = roadNetwork.Change(change, _idGenerator);
            if (result.Problems.HasError())
            {
                throw new RoadRegistryProblemsException(result.Problems);
            }
        }

        return roadNetwork;
    }

    public RoadNetworkBuilder Add(RoadNetworkChanges roadNetworkChanges)
    {
        _changes.Add(roadNetworkChanges);
        return this;
    }
}
