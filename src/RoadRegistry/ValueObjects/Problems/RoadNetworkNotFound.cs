namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadNetworkNotFound : Error
{
    public RoadNetworkNotFound()
        : base(ProblemCode.RoadNetwork.NotFound)
    {
    }
}
