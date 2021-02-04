namespace RoadRegistry.BackOffice.Api.Changes
{
    public class ChangeFeedEntry
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string TimeOfDay { get; set; }
    }
}
