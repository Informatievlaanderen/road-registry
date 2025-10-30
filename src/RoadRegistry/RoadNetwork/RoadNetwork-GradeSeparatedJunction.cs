namespace RoadRegistry.RoadNetwork;

using BackOffice.Core;
using Changes;
using ValueObjects;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;

public partial class RoadNetwork
{
    private Problems AddGradeSeparatedJunction(AddGradeSeparatedJunctionChange change, RoadNetworkChangeContext context)
    {
        var (gradeSeparatedJunction, problems) = GradeSeparatedJunction.Add(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        _gradeSeparatedJunctions.Add(gradeSeparatedJunction!.GradeSeparatedJunctionId, gradeSeparatedJunction);
        return problems;
    }

    // private Problems ModifyGradeSeparatedJunction(ModifyGradeSeparatedJunctionChange change, RoadNetworkChangeContext context)
    // {
    //     if (!_gradeSeparatedJunctions.TryGetValue(change.Id, out var gradeSeparatedJunction))
    //     {
    //         return Problems.Single(new GradeSeparatedJunctionNotFound(change.OriginalId ?? change.Id));
    //     }
    //
    //     return gradeSeparatedJunction.Modify(change, context);
    // }
    //
    // private Problems RemoveRoadSegment(RemoveGradeSeparatedJunctionChange change, RoadNetworkChangeContext context)
    // {
    //     if (!_gradeSeparatedJunctions.TryGetValue(change.Id, out var gradeSeparatedJunction))
    //     {
    //         return Problems.Single(new GradeSeparatedJunctionNotFound(change.Id));
    //     }
    //
    //     return gradeSeparatedJunction.Remove(change, context);
    // }
}
