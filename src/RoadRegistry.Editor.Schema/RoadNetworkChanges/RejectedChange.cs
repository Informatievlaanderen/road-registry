namespace RoadRegistry.Editor.Schema.RoadNetworkChanges;

public class RejectedChange
{
    public string Change { get; set; }

    public ProblemWithChange[] Problems { get; set; }
}