namespace RoadRegistry.BackOffice.Messages
{
    public class ChangeRoadNetworkBasedOnArchive
    {
        public string RequestId { get; set; }
        public string Reason { get; set; }
        public string Operator { get; set; }
        public string OrganizationId { get; set; }
        public RequestedChange[] Changes { get; set; }
    }
}
