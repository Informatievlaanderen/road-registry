namespace RoadRegistry.BackOffice.Core
{
    public class RoadNodeGeometryTaken : Warning
    {
        public RoadNodeGeometryTaken(RoadNodeId byOtherNode) : base(
            nameof(RoadNodeGeometryTaken),
            new ProblemParameter("ByOtherNode",
                byOtherNode.ToInt32().ToString()))
        {
        }
    }
}
