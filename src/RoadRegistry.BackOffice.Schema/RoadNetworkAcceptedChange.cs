namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkAcceptedChange
    {
        public string Change { get; set; }

        public RoadNetworkChangeProblem[] Problems { get; set; }
    }
}
