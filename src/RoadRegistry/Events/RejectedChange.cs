namespace RoadRegistry.Events
{
    public class RejectedChange
    {
        public string Change { get; set; }
        public int Id { get; set; }
        public string Reason { get; set; }
        public ReasonParameter[] Parameters { get; set; }
    }

}
