namespace RoadRegistry.BackOffice.Api.Changes;

public class ChangeFeedEntryContent
{
    public object Content { get; set; }
    public long Id { get; set; }
    public string Type { get; set; }
}
