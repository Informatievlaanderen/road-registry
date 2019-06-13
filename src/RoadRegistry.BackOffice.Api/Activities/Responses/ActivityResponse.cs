namespace RoadRegistry.Api.Activities.Responses
{
    using BackOffice.Schema;

    public class ActivityResponse
    {
        public ActivityResponseItem[] Activities { get; set; }
    }

    public class ActivityResponseItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public object Content { get; set; }
    }
}
