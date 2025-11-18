namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using BackOffice.Core;
using GradeSeparatedJunction.Changes;
using GradeSeparatedJunction = GradeSeparatedJunction.GradeSeparatedJunction;

public partial class RoadNetwork
{
    public IEnumerable<GradeSeparatedJunction> GetNonRemovedGradeSeparatedJunctions()
    {
        return _gradeSeparatedJunctions.Values.Where(x => !x.IsRemoved);
    }

    private Problems AddGradeSeparatedJunction(AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
    {
        var (gradeSeparatedJunction, problems) = GradeSeparatedJunction.Add(change, idGenerator, idTranslator);
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
