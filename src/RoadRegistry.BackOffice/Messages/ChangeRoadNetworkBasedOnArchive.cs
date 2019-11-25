namespace RoadRegistry.BackOffice.Messages
{
    using System;

    public class ChangeRoadNetworkBasedOnArchive
    {
        public String ArchiveId { get; set; }
        public RequestedChange[] Changes { get; set; }
    }
}
