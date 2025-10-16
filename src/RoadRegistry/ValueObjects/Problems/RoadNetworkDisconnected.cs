namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadNetworkDisconnected : Error
{
    public RoadNetworkDisconnected(RoadNodeId start, RoadNodeId end)
        : base(ProblemCode.RoadNetwork.Disconnected,
            new ProblemParameter("StartNodeId", start.ToString()),
            new ProblemParameter("EndNodeId", end.ToString()))
    {
    }
}
