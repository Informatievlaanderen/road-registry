namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

public class AcceptedChange
{
    public string Change { get; set; }
    public ProblemWithChange[] Problems { get; set; }
}