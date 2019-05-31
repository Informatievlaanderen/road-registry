namespace RoadRegistry.BackOffice.Messages
{
    public class FileProblem
    {
        public string File { get; set; }
        public FileProblemSeverity Severity { get; set; }
        public string Reason { get; set; }
        public ProblemParameter[] Parameters { get; set; }
    }

    public enum FileProblemSeverity
    {
        Error,
        Warning
    }
}
