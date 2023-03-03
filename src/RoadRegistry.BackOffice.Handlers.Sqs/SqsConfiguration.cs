namespace RoadRegistry.BackOffice.Handlers.Sqs;

public class SqsConfiguration: IHasConfigurationKey
{
    public string ServiceUrl { get; set; }
    
    public string GetConfigurationKey()
    {
        return "Sqs";
    }
}
