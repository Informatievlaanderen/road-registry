namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkRejectedChange
    {
        public string Change { get; set; }

        public RoadNetworkChangeProblem[] Problems { get; set; }
    }
}
