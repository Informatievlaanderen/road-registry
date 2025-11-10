namespace RoadRegistry.RoadNetwork;

using BackOffice.Core;
using GradeSeparatedJunction.Changes;
using ValueObjects;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;

public partial class RoadNetwork
{
    private Problems AddGradeSeparatedJunction(AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator)
    {
        var (gradeSeparatedJunction, problems) = GradeSeparatedJunction.Add(change, idGenerator);
        if (problems.HasError())
        {
            return problems;
        }

        _gradeSeparatedJunctions.Add(gradeSeparatedJunction!.GradeSeparatedJunctionId, gradeSeparatedJunction);
        return problems;
    }

    private Problems RemoveGradeSeparatedJunction(RemoveGradeSeparatedJunctionChange change)
    {
        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var gradeSeparatedJunction))
        {
            return Problems.Single(new GradeSeparatedJunctionNotFound(change.GradeSeparatedJunctionId));
        }

        return gradeSeparatedJunction.Remove();
    }
}
