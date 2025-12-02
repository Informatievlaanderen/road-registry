namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using GradeSeparatedJunction.Changes;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using GradeSeparatedJunction = GradeSeparatedJunction.GradeSeparatedJunction;

public partial class RoadNetwork
{
    public IEnumerable<GradeSeparatedJunction> GetNonRemovedGradeSeparatedJunctions()
    {
        return _gradeSeparatedJunctions.Values.Where(x => !x.IsRemoved);
    }

    private Problems AddGradeSeparatedJunction(RoadNetworkChanges changes, AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
    {
        var (gradeSeparatedJunction, problems) = GradeSeparatedJunction.Add(change, changes.Provenance, idGenerator, idTranslator);
        if (problems.HasError())
        {
            return problems;
        }

        _gradeSeparatedJunctions.Add(gradeSeparatedJunction!.GradeSeparatedJunctionId, gradeSeparatedJunction);
        summary.Added.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);

        return problems;
    }

    private Problems RemoveGradeSeparatedJunction(RoadNetworkChanges changes, RemoveGradeSeparatedJunctionChange change, RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> summary)
    {
        if (!_gradeSeparatedJunctions.TryGetValue(change.GradeSeparatedJunctionId, out var gradeSeparatedJunction))
        {
            return Problems.Single(new GradeSeparatedJunctionNotFound(change.GradeSeparatedJunctionId));
        }

        var problems = gradeSeparatedJunction.Remove(changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(gradeSeparatedJunction.GradeSeparatedJunctionId);
        return problems;
    }
}
