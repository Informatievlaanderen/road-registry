namespace RoadRegistry.Api.Changes.Responses
{
    public class ChangeFeedEntry
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public object Content { get; set; }
    }
}