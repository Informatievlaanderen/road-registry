namespace RoadRegistry.BackOffice.Messages
{
    public class Problem
    {
        public string Reason { get; set; }
        public ProblemParameter[] Parameters { get; set; }
    }
}
