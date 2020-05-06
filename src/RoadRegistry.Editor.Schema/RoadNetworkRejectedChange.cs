namespace RoadRegistry.Editor.Schema
{
    public class RoadNetworkRejectedChange
    {
        public string Change { get; set; }

        public RoadNetworkChangeProblem[] Problems { get; set; }
    }
}
