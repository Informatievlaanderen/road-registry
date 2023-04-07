namespace RoadRegistry.BackOffice.Configuration;

public class SqsConfiguration: IHasConfigurationKey
{
    public string ServiceUrl { get; set; }
    
    public string GetConfigurationKey()
    {
        return "Sqs";
    }
}
