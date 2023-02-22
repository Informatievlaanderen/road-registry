namespace RoadRegistry.BackOffice.Configuration;

public class SqsQueueUrlOptions: IHasConfigurationKey
{
    public string BackOffice { get; set; }
    public string Snapshot { get; set; }

    public string GetConfigurationKey()
    {
        return "SqsQueueUrlOptions";
    }
}
