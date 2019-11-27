namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkRejectedChange
    {
        public string Change { get; set; }

        public RoadNetworkRejectedChangeProblem[] Problems { get; set; }
    }
}