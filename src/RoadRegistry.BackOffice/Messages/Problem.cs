namespace RoadRegistry.BackOffice.Messages;

public class Problem
{
    public ProblemParameter[] Parameters { get; set; }
    public string Reason { get; set; }
    public ProblemSeverity Severity { get; set; }
}
