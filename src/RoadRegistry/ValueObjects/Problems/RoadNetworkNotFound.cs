namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadNetworkNotFound : Error
{
    public RoadNetworkNotFound()
        : base(ProblemCode.RoadNetwork.NotFound)
    {
    }
}
