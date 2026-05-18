namespace RoadRegistry.Extracts.DataValidation;

using RoadRegistry.BackOffice;

public class DataValidationOptions : IHasConfigurationKey
{
    public string ApiBaseUrl { get; set; }
    public string SpecificationCode { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string TokenEndPoint { get; set; }

    public string GetConfigurationKey()
    {
        return "DataValidationOptions";
    }
}
