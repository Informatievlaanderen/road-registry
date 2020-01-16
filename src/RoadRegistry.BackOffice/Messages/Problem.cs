namespace RoadRegistry.BackOffice.Messages
{
    public class Problem
    {
        public ProblemSeverity Severity { get; set; }
        public string Reason { get; set; }
        public ProblemParameter[] Parameters { get; set; }
    }
}
