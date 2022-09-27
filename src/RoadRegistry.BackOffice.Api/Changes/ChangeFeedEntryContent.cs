namespace RoadRegistry.BackOffice.Api.Changes;

public class ChangeFeedEntryContent
{
    public long Id { get; set; }
    public string Type { get; set; }
    public object Content { get; set; }
}
